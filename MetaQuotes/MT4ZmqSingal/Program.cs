using System;
using System.Text;
using Trady.Core.Period;
using ZeroMQ;
using FixEAStrategy;
using System.Linq;
using Trady.Analysis.Extension;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using System.IO;
using System.Collections.Concurrent;

namespace MT4ZmqSingal
{
    class Program
    {
        static CancellationTokenSource CloseSource = new CancellationTokenSource();

        #region 共享数据
        static readonly double OldDataOffsetHours = 2.0;  //静态数据文件UTC时差
        static readonly string TestServerName = "ICMarkets-Demo03";
        static readonly string TestSymbol = "XAUUSD";

        static ZmqCandle SecondCandles = new ZmqCandle(TestSymbol, new PerSecond(), 60 * 60);
        static ZmqCandle M1Candles = new ZmqCandle(TestSymbol, new PerMinute());
        static ZmqCandle M5Candles = new ZmqCandle(TestSymbol, new Per5Minute());
        static ZmqCandle M15Candles = new ZmqCandle(TestSymbol, new Per15Minute());
        static ZmqCandle M30Candles = new ZmqCandle(TestSymbol, new Per30Minute());
        static ZmqCandle H1Candles = new ZmqCandle(TestSymbol, new Hourly(), 1000);
        static ZmqCandle H4Candles = new ZmqCandle(TestSymbol, new FourHourly(), 100);

        static ILog logTA = LogManager.GetLogger("TALog");
        static ILog logCmd = LogManager.GetLogger("CmdLog");
        static ILog logData = LogManager.GetLogger("DataLog");
        static ILog log = LogManager.GetLogger("ColoredConsoleAppender");


        static ConcurrentQueue<IMetaTraderCommand> MTCommandQueue = new ConcurrentQueue<IMetaTraderCommand>(); //待处理命令

        #endregion

        static void Main(string[] args)
        {
            bool isLiveTime = true;
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));

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

            new ThreadTask("数据接收", CollectDataTask).Start();
            new ThreadTask("信号分析", TradeSingleAnalysize).Start();
            new ThreadTask("命令执行", TradeCommandProcessor).Start();
            Console.Title = string.Format("主线程{0}运行中, 输入字符'q'退出程序.", Thread.CurrentThread.ManagedThreadId);

        WaitClose:
            string q = Console.ReadLine();
            while (q != "q")
            {
                if (q == "b")
                {
                    string.Format("【S】{0} 【M1】{1} 【M5】{2} 【M15】{3} 【M30】{4} 【H1】{5}",
                        SecondCandles.Count(),
                        M1Candles.Count().ToString() + " / " + M1Candles.NewBarPercent(isLiveTime).ToString("P2"),
                        M5Candles.Count().ToString() + " / " + M5Candles.NewBarPercent(isLiveTime).ToString("P2"),
                        M15Candles.Count().ToString() + " / " + M15Candles.NewBarPercent(isLiveTime).ToString("P2"),
                        M30Candles.Count().ToString() + " / " + M30Candles.NewBarPercent(isLiveTime).ToString("P2"),
                        H1Candles.Count().ToString() + " / " + H1Candles.NewBarPercent(isLiveTime).ToString("P2")).ConsoleLineReplace();
                }
                else if (q == "cls")
                {
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine(q + " -> 未知交互命令！");
                }
                goto WaitClose;
            }
            CloseSource.Cancel();

            DateTime closeTime = DateTime.Now.AddSeconds(20);
            while (closeTime.Subtract(DateTime.Now).TotalSeconds > 0)
            {
                ("等待" + closeTime.Subtract(DateTime.Now).TotalSeconds.ToString("N0") + "秒后退出").ConsoleLineReplace();
                Thread.Sleep(1000);
            }
        }

        static void TradeSingleAnalysize()
        {
            while (!CloseSource.IsCancellationRequested)
            {
                //Console.WriteLine("#{3}:{0}, Pool:{1}, State:{2} 循环运行中 {4}", Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread, Thread.CurrentThread.ThreadState, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("mm:ss,fff"));
                //logTA.DebugFormat("S:{0} M1:{1} M5:{2} M15:{3} M30:{4} H1:{5}",
                //    SecondCandles.Count(), M1Candles.Count(), M5Candles.Count(), M15Candles.Count(), M30Candles.Count(), H1Candles.Count());

                //int testCount = 5;
                //M1Candles.LastNCandles(testCount).ToList().ForEach(c =>
                //{
                //    int offset = 1 * 60;
                //    var mySeconds = SecondCandles.Where(s => s.DateTime >= c.DateTime && s.DateTime < c.DateTime.AddSeconds(offset)).ToList();
                //    logTA.InfoFormat("[{0}]({5}) O:{1} H:{2} L:{3} C:{4}", c.DateTime, c.Open, c.High, c.Low, c.Close, mySeconds.Count);
                //});

                #region DemoCommand
                //jsonCommand = "MT-ORDER {\"Pool\":1}";
                //jsonCommand = "MT-ORDER {\"Pool\":1, \"Index\":116248227, \"Select\":1 }";
                #endregion

                //iBands
                using (CommandExecContext ctx1 = new CommandExecContext())
                {
                    IMetaTraderCommand cmd1 = new TAIBandsCommand(TestSymbol, "M1", 5);
                    cmd1.Context = ctx1;

                    MTCommandQueue.Enqueue(cmd1);

                    ctx1.ResetEvent.WaitOne();
                    object response1 = cmd1.Response;
                    logCmd.Debug(response1);
                    ctx1.ResetEvent.Reset();
                }

                //Stochastic
                using (CommandExecContext ctx2 = new CommandExecContext())
                {
                    IMetaTraderCommand cmd2 = new TAStochasticCommand(TestSymbol, "M1", 5);
                    cmd2.Context = ctx2;

                    MTCommandQueue.Enqueue(cmd2);

                    ctx2.ResetEvent.WaitOne();
                    object response2 = cmd2.Response;
                    logCmd.Debug(response2);
                    ctx2.ResetEvent.Reset();
                }

                //OHLC
                using (CommandExecContext ctx3 = new CommandExecContext())
                {
                    IMetaTraderCommand cmd3 = new TAOHLCCommand(TestSymbol, "M1", 5);
                    cmd3.Context = ctx3;

                    MTCommandQueue.Enqueue(cmd3);

                    ctx3.ResetEvent.WaitOne();
                    object response3 = cmd3.Response;
                    logCmd.Debug(response3);
                    ctx3.ResetEvent.Reset();
                }

                #region Trady指标 MEMO
                //int candleCount = M5Candles.Count();
                //if (candleCount >= 5)
                //{
                //    //var last = M5Candles.Sma(15).Last();
                //    string numFmt = "#." + "".PadLeft(2, '#');
                //    //Console.WriteLine($"{last.DateTime}, {last.Tick.Value.ToString(numFmt)}");

                //    if (candleCount >= 20)
                //    {
                //        var bb = M5Candles.Bb(20, 2).Last();
                //        Console.WriteLine($"Bands Upper: {bb.Tick.UpperBand.Value.ToString(numFmt)}, Middle: {bb.Tick.MiddleBand.Value.ToString(numFmt)}, Lower: {bb.Tick.LowerBand.Value.ToString(numFmt)}");
                //    }

                //    var sto = M5Candles.FastSto(5, 3).Last();
                //    Console.WriteLine($"K: {sto.Tick.K.Value.ToString(numFmt)}, D: {sto.Tick.D.Value.ToString(numFmt)}, J: {sto.Tick.J.Value.ToString(numFmt)}");

                //    var stoFlow = M5Candles.FastStoOsc(5, 3).Last();
                //    var stoSlow = M5Candles.SlowStoOsc(5, 3).Last();
                //    Console.WriteLine($"Fast: {stoFlow.Tick.Value.ToString(numFmt)}, Slow: {stoSlow.Tick.Value.ToString(numFmt)}");

                //}
                #endregion
            }

            Console.WriteLine("==> #{1}({0}) 执行完成", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
        }

        static void CollectDataTask()
        {
            #region ZMQContext
            using (var context = ZmqContext.Create())
            {
                using (var subSocket = context.CreateSocket(SocketType.SUB))
                {
                    string address = "tcp://localhost:5556";
                    subSocket.Connect(address);
                    string prefixString = string.Concat(TestServerName, "|", TestSymbol, "|");
                    byte[] prefixBytes = Encoding.Default.GetBytes(prefixString);
                    subSocket.Subscribe(prefixBytes);

                    string[] tickArr = new string[0];
                    string[] priceArr = new string[3];
                    int digits = 2;
                    while (!CloseSource.IsCancellationRequested)
                    {
                        #region 循环处理消息
                        var message = subSocket.Receive(Encoding.Default);
                        string usefulMsg = message.Substring(prefixString.Length);

                        tickArr = usefulMsg.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        string rightTickStr = Regex.Replace(tickArr[0], "\\(GMT([+-]\\d{1,2})*\\)", "");
                        DateTimeOffset tickTime = new DateTimeOffset(DateTime.ParseExact(rightTickStr,
                              (rightTickStr.Contains(",") ? "yyyy.MM.dd HH:mm:ss,fff" : "yyyy.MM.dd HH:mm:ss"),
                              CultureInfo.InvariantCulture),
                              (message.Contains("(GMT)") ? TimeSpan.Zero : TimeSpan.FromHours(OldDataOffsetHours))
                            );

                        digits = int.Parse(tickArr[1]);
                        priceArr = tickArr[2].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        Decimal bid, ask;
                        bid = decimal.Parse(priceArr[0]);
                        ask = decimal.Parse(priceArr[1]);
                        SecondCandles.NewTickData(tickTime, bid, ask, 0.0m);
                        M1Candles.NewTickData(tickTime, bid, ask, 0.0m);
                        M5Candles.NewTickData(tickTime, bid, ask, 0.0m);
                        M15Candles.NewTickData(tickTime, bid, ask, 0.0m);
                        M30Candles.NewTickData(tickTime, bid, ask, 0.0m);
                        H1Candles.NewTickData(tickTime, bid, ask, 0.0m);
                        H4Candles.NewTickData(tickTime, bid, ask, 0.0m);
                        #endregion
                    }
                }
            }
            #endregion
            Console.WriteLine("==> #{1}({0}) 执行完成", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
        }

        static void TradeCommandProcessor()
        {
            using (var context = ZmqContext.Create())
            {
                using (var svrSocket = context.CreateSocket(SocketType.REQ))
                {
                    svrSocket.Connect("tcp://localhost:5555");
                    while (!CloseSource.IsCancellationRequested)
                    {
                        //Console.WriteLine("#{3}:{0}, Pool:{1}, State:{2} 循环运行中 {4}", Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread, Thread.CurrentThread.ThreadState, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("mm:ss,fff"));
                        if (MTCommandQueue.Count == 0)
                        {
                            Thread.Sleep(50);
                        }
                        else
                        {
                            while (MTCommandQueue.Count > 0)
                            {
                                IMetaTraderCommand cmd = null;
                                if (MTCommandQueue.TryDequeue(out cmd))
                                {
                                    #region 命令处理
                                    string jsonCommand = string.Concat(cmd.CommandName, " ", cmd.GetParameterJson());
                                    SendStatus status = svrSocket.Send(jsonCommand, Encoding.Default);
                                    if (status == SendStatus.Sent)
                                    {
                                        string result = string.Empty;
                                        result = svrSocket.Receive(Encoding.UTF8);
                                        try
                                        {
                                            cmd.JsonCallBack(result);
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }
                                    #endregion
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine("==> #{1}({0}) 执行完成", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
        }
    }
}
