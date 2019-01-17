﻿using System;
using System.Collections.Generic;
using System.Text;
using Trady.Core.Period;
using ZeroMQ;
using Trady.Analysis.Extension;
using System.Linq;
using System.Globalization;

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

        }

        static void ZmqCandleLive()
        {
            ZmqCandle candles = new ZmqCandle("XAUUSD", new PerMinute());
            //ZmqCandle candles = new ZmqCandle("XAUUSD", new PerSecond());
            using (var context = ZmqContext.Create())
            {
                using (var subSocket = context.CreateSocket(SocketType.SUB))
                {
                    string address = "tcp://localhost:5556";
                    subSocket.Connect(address);
                    Console.WriteLine("Listening on " + address);
                    Console.WriteLine();
                    Console.WriteLine("Press ctrl+c to exit...");
                    Console.WriteLine();

                    //ICMarkets-Demo03|XAUUSD|(GMT)2019.01.17 12:53:02|2|1293.98:1294.05:7
                    string prefixString = string.Concat("ICMarkets-Demo03|", candles.Symbol(), "|");
                    byte[] prefixBytes = Encoding.Default.GetBytes(prefixString);
                    subSocket.Subscribe(prefixBytes);
                    //subSocket.SubscribeAll();

                    string[] tickArr = new string[0];
                    string[] priceArr = new string[3];
                    int digits = 2;
                    string numFmt = "##.##";
                    while (true)
                    {
                        var message = subSocket.Receive(Encoding.UTF8);
                        string usefulMsg = message.Substring(prefixString.Length);
                        //Console.WriteLine(usefulMsg);

                        tickArr = usefulMsg.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        DateTimeOffset tickTime = new DateTimeOffset(DateTime.Parse(tickArr[0].Replace("(GMT)", "")), TimeSpan.Zero);

                        digits = int.Parse(tickArr[1]);

                        priceArr = tickArr[2].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        candles.NewTickData(tickTime, decimal.Parse(priceArr[0]), decimal.Parse(priceArr[1]), 0.0m);

                        int candleCount = candles.Count();
                        if (candleCount > 5)
                        {
                            //var last = candles.Sma(15).Last();
                            ////numFmt = "#." + "".PadLeft(digits, '#');
                            //Console.WriteLine($"{last.DateTime}, {last.Tick.Value.ToString(numFmt)}");

                            if (candleCount > 20)
                            {
                                var bb = candles.Bb(20, 2).Last();
                                Console.WriteLine($"Bands Upper: {bb.Tick.UpperBand.Value.ToString(numFmt)}, Middle: {bb.Tick.MiddleBand.Value.ToString(numFmt)}, Lower: {bb.Tick.LowerBand.Value.ToString(numFmt)}");
                            }

                            var sto = candles.FastSto(5, 3).Last();
                            Console.WriteLine($"K: {sto.Tick.K.Value.ToString(numFmt)}, D: {sto.Tick.D.Value.ToString(numFmt)}, J: {sto.Tick.J.Value.ToString(numFmt)}");

                            var stoFlow = candles.FastStoOsc(5, 3).Last();
                            var stoSlow = candles.SlowStoOsc(5, 3).Last();
                            Console.WriteLine($"Fast: {stoFlow.Tick.Value.ToString(numFmt)}, Slow: {stoSlow.Tick.Value.ToString(numFmt)}");

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
