using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;
using Trady.Core.Period;
using System.IO;
using Trady.Core;
using FixEAStrategy;

namespace TickDataImporter
{
    public class TickDataFileImporter : IImporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TickDataFileImporter"/> class.
        /// </summary>
        /// <param name="tickDir">tick数据基础目录</param>
        /// <param name="symbolPattern">包含符号，日期，扩展名的正则匹配模式</param>
        public TickDataFileImporter(string tickDir, string symbolPattern)
        {
            TickDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tickDir);
            SymbolPattern = symbolPattern;
        }

        string TickDataPath;
        string SymbolPattern; //(?<symbol>[\\w]+)_Tick_(?<date>\\d{4}.\\d{2}.\\d{2})(?<ext>.dat)"

        /// <summary>
        /// 导入M1数据
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="period">The period.</param>
        /// <param name="token">The token.</param>
        /// <returns>Task&lt;IReadOnlyList&lt;IOhlcv&gt;&gt;.</returns>
        public async Task<IReadOnlyList<IOhlcv>> ImportAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null,
            PeriodOption period = PeriodOption.Daily, CancellationToken token = default(CancellationToken))
        {

            return await Task.Factory.StartNew(() =>
            {
                if (period != PeriodOption.PerMinute)
                    throw new NotSupportedException();

                var candles = new List<IOhlcv>();
                string[] allFiles = Directory.GetFiles(TickDataPath);
                foreach (var myFile in allFiles)
                {
                    Match m = Regex.Match(myFile, SymbolPattern);
                    if (m.Success)
                    {
                        string fileSymbol = m.Groups["symbol"].Value;
                        if (fileSymbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase))
                        {
                            string dateStr = m.Groups["date"].ToString();
                            DateTime fileDatDay = DateTime.ParseExact(dateStr, "yyyy.MM.dd", System.Globalization.CultureInfo.CurrentCulture);

                            if (startTime != null && period == PeriodOption.PerMinute
                                && fileDatDay != startTime.Value.Date)
                            {
                                continue;
                            }

                            #region 单个匹配文件导入
                            using (FileStream fs = new FileStream(myFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                byte[] bufBytes = new byte[64];

                                int iIntVer = 0;
                                long iLongVal = 0L;
                                var nTickTime = DateTime.Now;

                             RateInfo:

                                iLongVal = fs.ReadLong(ref bufBytes); //time_msc (8)
                                nTickTime = iLongVal.ToDateTime();
                                iIntVer = fs.ReadInt(ref bufBytes);   //tTick    (4)
                                /*
                                    The GetTickCount() function returns the number of milliseconds that elapsed since the system start.
                                    uint  GetTickCount();
                                */
                                var tick = new TickRate
                                {
                                    TickTime = nTickTime,
                                    Bid = fs.ReadDouble(ref bufBytes), //bid     (8)
                                    Ask = fs.ReadDouble(ref bufBytes), //ask     (8)
                                    Volume = fs.ReadLong(ref bufBytes) //volume  (8)
                                };

                                var nCandle = tick.ToCandle();

                                if (!candles.Any())
                                {
                                    candles.Add(nCandle);
                                }
                                else
                                {
                                    var last = (MT4Candle)candles.Last();
                                    switch (period)
                                    {
                                        case PeriodOption.PerSecond:
                                            break;
                                        case PeriodOption.PerMinute:
                                            if (last.DateTime.DateTime.ClearSeconds() == nTickTime.ClearSeconds())
                                            {
                                                last.MerginTickData(tick);
                                            }
                                            else
                                            {
                                                candles.Add(nCandle);
                                            }
                                            break;
                                        case PeriodOption.Per5Minute:
                                            break;
                                        case PeriodOption.Per10Minute:
                                            break;
                                        case PeriodOption.Per15Minute:
                                            break;
                                        case PeriodOption.Per30Minute:
                                            break;
                                        case PeriodOption.Hourly:
                                            break;
                                        case PeriodOption.BiHourly:
                                            break;
                                        case PeriodOption.Daily:
                                            break;
                                        case PeriodOption.Weekly:
                                            break;
                                        case PeriodOption.Monthly:
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                if (fs.Position + 36 < fs.Length - 1)
                                {
                                    goto RateInfo;
                                }
                            }
                            #endregion
                        }
                    }
                }
                return candles.ToList();

            });


        }
    }
}
