using System;
using System.Collections.Generic;
using System.Text;
using Trady.Core.Period;
using ZeroMQ;
using FixEAStrategy;
using System.Linq;
using Trady.Analysis;
using Trady.Core;
using Trady.Analysis.Extension;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MT4ZmqSingal
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length < 1)
            //    ShowUsage();
            //SubscribeToMessages(args); 

            ZmqCandleLive();

            #region TODO

            /*
             * //https://lppkarl.github.io/Trady/getting_started.html
             * 
             Step 4: Create Buy/Sell Rules
                var buyRule = Rule.Create(c => c.IsFullStoBullishCross(14, 3, 3))
                    .And(c => c.IsMacdOscBullish(12, 26, 9))
                    .And(c => c.IsAccumDistBullish());

                var sellRule = Rule.Create(c => c.IsFullStoBearishCross(14, 3, 3))
                    .Or(c => c.IsMacdBearishCross(12, 24, 9));

            Step 5: Create Portfolio Builder
            var runner = new Builder()
                .Add(fb, 10)
                .Buy(buyRule)
                .Sell(sellRule)
                .Build();

            Step 6: Start Backtesting with the Portfolio
            var result = await runner.RunAsync(10000, 1);

            Step 7: Review Backtest Results
            Console.WriteLine(string.Format("Transaction count: {0:#.##}, P/L ratio: {1:0.##}%, Principal: {2:#}, Total: {3:#}",
                result.Transactions.Count(),
                result.CorrectedProfitLoss * 100,
                result.Principal,
                result.CorrectedBalance));
             */

            #endregion

            //ZmqCmdServerLive();
        }

        static void ZmqCandleLive()
        {
            ZmqCandle M1Candles = new ZmqCandle("XAUUSD", new PerMinute());
            ZmqCandle M5Candles = new ZmqCandle("XAUUSD", new Per5Minute());
            ZmqCandle M15Candles = new ZmqCandle("XAUUSD", new Per15Minute());
            ZmqCandle M30Candles = new ZmqCandle("XAUUSD", new Per30Minute());
            ZmqCandle H1Candles = new ZmqCandle("XAUUSD", new Hourly());
            using (var context = ZmqContext.Create())
            {
                using (var subSocket = context.CreateSocket(SocketType.SUB))
                {
                    string address = "tcp://localhost:5556";
                    subSocket.Connect(address);
                    Console.WriteLine("Subscrible on " + address);
                    Console.WriteLine();
                    Console.WriteLine("Press ctrl+c to exit...");
                    Console.WriteLine();

                    //ICMarkets-Demo03|XAUUSD|(GMT)2019.01.17 12:53:02|2|1293.98:1294.05:7
                    string prefixString = string.Concat("ICMarkets-Demo03|", M1Candles.Symbol(), "|");
                    byte[] prefixBytes = Encoding.Default.GetBytes(prefixString);
                    subSocket.Subscribe(prefixBytes);
                    //subSocket.SubscribeAll();

                    string[] tickArr = new string[0];
                    string[] priceArr = new string[3];
                    int digits = 2;
                    string numFmt = "##.##";
                    while (true)
                    {
                        #region 循环处理消息
                        var message = subSocket.Receive(Encoding.Default);
                        //Console.WriteLine(message);
                        //message.ConsoleLineReplace();

                        string usefulMsg = message.Substring(prefixString.Length);

                        tickArr = usefulMsg.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        string rightTickStr = Regex.Replace(tickArr[0], "\\(GMT([+-]\\d{1,2})*\\)", "");
                        DateTimeOffset tickTime = new DateTimeOffset(DateTime.ParseExact(rightTickStr, "yyyy.MM.dd HH:mm:ss,fff", CultureInfo.InvariantCulture), TimeSpan.FromHours(2.0));

                        digits = int.Parse(tickArr[1]);
                        priceArr = tickArr[2].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                        M1Candles.NewTickData(tickTime, decimal.Parse(priceArr[0]), decimal.Parse(priceArr[1]), 0.0m);
                        M5Candles.NewTickData(tickTime, decimal.Parse(priceArr[0]), decimal.Parse(priceArr[1]), 0.0m);
                        M15Candles.NewTickData(tickTime, decimal.Parse(priceArr[0]), decimal.Parse(priceArr[1]), 0.0m);
                        M30Candles.NewTickData(tickTime, decimal.Parse(priceArr[0]), decimal.Parse(priceArr[1]), 0.0m);
                        H1Candles.NewTickData(tickTime, decimal.Parse(priceArr[0]), decimal.Parse(priceArr[1]), 0.0m);

                        int candleCount = M1Candles.Count();
                        if (candleCount >= 5)
                        {
                            //var last = candles.Sma(15).Last();
                            ////numFmt = "#." + "".PadLeft(digits, '#');
                            //Console.WriteLine($"{last.DateTime}, {last.Tick.Value.ToString(numFmt)}");

                            if (candleCount >= 20)
                            {
                                var bb = M1Candles.Bb(20, 2).Last();
                                Console.WriteLine($"Bands Upper: {bb.Tick.UpperBand.Value.ToString(numFmt)}, Middle: {bb.Tick.MiddleBand.Value.ToString(numFmt)}, Lower: {bb.Tick.LowerBand.Value.ToString(numFmt)}");
                            }

                            var sto = M1Candles.FastSto(5, 3).Last();
                            Console.WriteLine($"K: {sto.Tick.K.Value.ToString(numFmt)}, D: {sto.Tick.D.Value.ToString(numFmt)}, J: {sto.Tick.J.Value.ToString(numFmt)}");

                            var stoFlow = M1Candles.FastStoOsc(5, 3).Last();
                            var stoSlow = M1Candles.SlowStoOsc(5, 3).Last();
                            Console.WriteLine($"Fast: {stoFlow.Tick.Value.ToString(numFmt)}, Slow: {stoSlow.Tick.Value.ToString(numFmt)}");
                        }
                        else
                        {
                            (string.Concat(usefulMsg, " (", candleCount, "/M5)")).ConsoleLineReplace();
                            //Console.WriteLine(usefulMsg);
                        }
                        #endregion
                    }
                }
            }
        }

        static void ZmqCmdServerLive()
        {
            using (var context = ZmqContext.Create())
            {
                using (var svrSocket = context.CreateSocket(SocketType.REQ))
                {
                    string address = "tcp://localhost:5555";
                    svrSocket.Connect(address);
                    Console.WriteLine("Listening on " + address);
                    Console.WriteLine();
                    Console.WriteLine("Press ctrl+c to exit...");
                    Console.WriteLine();
                    while (true)
                    {
                        string result = string.Empty;
                        SendStatus status = svrSocket.Send("BBands", Encoding.Default);
                        if (status == SendStatus.Sent)
                        {
                            result = svrSocket.Receive(Encoding.Default);
                            Console.WriteLine(result);
                        }

                        status = svrSocket.Send("Sto", Encoding.Default);
                        if (status == SendStatus.Sent)
                        {
                            result = svrSocket.Receive(Encoding.Default);
                            Console.WriteLine(result);
                        }

                        status = svrSocket.Send("OHLC", Encoding.Default);
                        if (status == SendStatus.Sent)
                        {
                            result = svrSocket.Receive(Encoding.Default);
                            Console.WriteLine(result);
                        }
                    }
                }
            }
        }

        private static void SubscribeToMessages(IEnumerable<string> addresses)
        {
            using (var context = ZmqContext.Create())
            {
                using (var subSocket = context.CreateSocket(SocketType.SUB))
                {

                    Console.WriteLine();
                    foreach (var address in addresses)
                    {
                        subSocket.Connect(address);
                        Console.WriteLine("Listening on " + address);
                    }
                    Console.WriteLine();
                    Console.WriteLine("Press ctrl+c to exit...");
                    Console.WriteLine();

                    subSocket.SubscribeAll();
                    while (true)
                    {
                        var message = subSocket.Receive(Encoding.UTF8);
                        Console.WriteLine(message);
                        //Console.WriteLine();
                    }
                }
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine();
            Console.WriteLine("ConsoleZmqSubscriber");
            Console.WriteLine();
            Console.WriteLine("Usage");
            Console.WriteLine("  consoleZmqSubscriber.exe <address> [<address>] [<address>] ...");
            Console.WriteLine();
            Console.WriteLine("e.g. consoleZmqSubscriber.exe tcp://127.0.0.1:6000 tcp://127.0.0.1:6001");
            Console.WriteLine();
        }
    }
}
