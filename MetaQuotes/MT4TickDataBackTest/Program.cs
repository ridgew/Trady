using System;
using System.IO;
using System.Text;
using ZeroMQ;
using FixEAStrategy;

namespace MT4TickDataBackTest
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var context = ZmqContext.Create())
            {
                using (var publisher = context.CreateSocket(SocketType.PUB))
                {
                    string address = "tcp://*:5556";
                    publisher.Bind(address);
                    Console.WriteLine("价格发布绑定到 -> " + address);
                    Console.WriteLine();
                    Console.WriteLine("Press ctrl+c to exit...");
                    Console.WriteLine();
                    string myFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XAUUSD_Tick_2019.01.18.dat");

                    #region 单个文件发布
                    using (FileStream fs = new FileStream(myFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        byte[] bufBytes = new byte[64];

                        int iIntVer = 0;
                        long iLongVal = 0L;
                        var nTickTime = DateTime.Now;
                        DateTime lastTickTime = DateTime.Parse("1970-01-01 00:00:00");

                    RateInfo:

                        iLongVal = fs.ReadLong(ref bufBytes); //time_msc (8)
                        nTickTime = iLongVal.ToDateTime();
                        iIntVer = fs.ReadInt(ref bufBytes);   //tTick    (4)
                                                              /*
                                                                  The GetTickCount() function returns the number of milliseconds that elapsed since the system start.
                                                                  uint  GetTickCount();
                                                              */
                        string tms = iIntVer.ToString();
                        var tick = new TickRate
                        {
                            TickTime = nTickTime,
                            Bid = fs.ReadDouble(ref bufBytes), //bid     (8)
                            Ask = fs.ReadDouble(ref bufBytes), //ask     (8)
                            Volume = fs.ReadLong(ref bufBytes) //volume  (8)
                        };

                        string tickTimeWithMs = string.Concat(nTickTime.ToString("yyyy.MM.dd HH:mm:ss"), ",", tms.Substring(tms.Length - 3, 3));
                        DateTime currentTickTime = DateTime.ParseExact(tickTimeWithMs, "yyyy.MM.dd HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture);
                        double totalMs = currentTickTime.Subtract(lastTickTime).TotalMilliseconds;
                        int sleepMs = 0;
                        if (totalMs > 5.0)
                        {
                            sleepMs = (int)totalMs;
                            if (lastTickTime.Year != 1970)
                                System.Threading.Thread.Sleep(sleepMs);
                            lastTickTime = currentTickTime;
                        }

                        string msg = string.Concat("ICMarkets-Demo03|", "XAUUSD",
                            "|", "(GMT+2)", tickTimeWithMs,
                            "|2|",
                            tick.Bid.ToString("#.##"), ":", tick.Ask.ToString("#.##"), ":",
                            ((tick.Ask - tick.Bid) * 100).ToString("N0"));

                        SendStatus status = publisher.Send(msg, Encoding.Default);
                        if (status != SendStatus.Sent)
                        {
                            //System.Threading.Thread.Sleep(2000);
                            Console.WriteLine("\r\n 发布出错：" + status);
                        }

                        string.Concat(msg, " ", ((double)fs.Position / (double)fs.Length).ToString("P2")).ConsoleLineReplace();
                        if (fs.Position + 36 < fs.Length - 1)
                        {
                            goto RateInfo;
                        }
                    }
                    #endregion
                }
            }

            Console.WriteLine("\r\n发布完成！");
            Console.Read();

        }
    }
}
