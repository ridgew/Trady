using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FixEAStrategy;
using Trady.Core;
using Trady.Core.Infrastructure;
using Trady.Core.Period;

namespace MT4ZmqSingal
{
    [DebuggerDisplay("Period:{Period} Total:{TotalCandle()}")]
    public class ZmqCandle : IEnumerable<IOhlcv>
    {
        public ZmqCandle(string symbol, PeriodBase period)
        {
            _symbol = symbol;
            Period = period;
            myCandleQue = new LimitedQueue<IOhlcv>(65535);
        }

        public ZmqCandle(string symbol, PeriodBase period, int queueSize)
        {
            _symbol = symbol;
            Period = period;
            myCandleQue = new LimitedQueue<IOhlcv>(queueSize);
        }

        string _symbol = null;
        LimitedQueue<IOhlcv> myCandleQue;

        public string Symbol()
        {
            return _symbol;
        }

        public PeriodBase Period { get; private set; }

        public IEnumerator<IOhlcv> GetEnumerator()
        {
            return myCandleQue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return myCandleQue.GetEnumerator();
        }

        public void NewTickData(DateTimeOffset tickTime, decimal bid, decimal ask, decimal lastVolume)
        {
            MT4Candle last = null;
            bool isExistsCandle = ExistsQueueCandle(tickTime, ref last);
            if (!isExistsCandle)
            {
                MT4Candle candle = new MT4Candle(FixedCandleStartTime(tickTime), bid, bid, ask, bid, 0);
                myCandleQue.Enqueue(candle);
            }
            else
            {
                TickRate tick = new TickRate { Ask = ask, Bid = bid, TickTime = tickTime.DateTime, Volume = lastVolume };
                last.MerginTickData(tick);
            }
        }

        public Tuple<DateTimeOffset, DateTimeOffset> TimeRange()
        {
            if (myCandleQue.Count() > 0)
                return new Tuple<DateTimeOffset, DateTimeOffset>(myCandleQue.First().DateTime, myCandleQue.Last().DateTime);

            return new Tuple<DateTimeOffset, DateTimeOffset>(new DateTimeOffset(), new DateTimeOffset());
        }

        DateTimeOffset FixedCandleStartTime(DateTimeOffset tickTime)
        {
            if (Period is IntradayPeriodBase)
            {
                #region IntradayPeriod
                var truePeriod = (IntradayPeriodBase)Period;
                if (truePeriod.NumberOfSecond >= 4 * 60 * 60)
                {
                    #region H4
                    //01 05 09
                    int offset = tickTime.DateTime.Hour % 4;
                    if (offset == 0)
                    {
                        return new DateTimeOffset(tickTime.DateTime.ClearMinutes().Subtract(TimeSpan.FromHours(3.0)), tickTime.Offset);
                    }
                    else
                    {
                        return new DateTimeOffset(tickTime.DateTime.ClearMinutes().Subtract(TimeSpan.FromHours(offset - 1)), tickTime.Offset);
                    }
                    #endregion
                }
                else if (truePeriod.NumberOfSecond >= 2 * 60 * 60)
                {
                    #region H2
                    //01 03 05
                    int offset = tickTime.DateTime.Hour % 2;
                    if (offset == 0)
                    {
                        return new DateTimeOffset(tickTime.DateTime.ClearMinutes().Subtract(TimeSpan.FromHours(1.0)), tickTime.Offset);
                    }
                    else
                    {
                        return new DateTimeOffset(tickTime.DateTime.ClearMinutes(), tickTime.Offset);
                    }
                    #endregion
                }
                else if (truePeriod.NumberOfSecond >= 60 * 60)
                {
                    #region H1
                    return new DateTimeOffset(tickTime.DateTime.ClearMinutes(), tickTime.Offset);
                    #endregion
                }
                else if (truePeriod.NumberOfSecond >= 30 * 60)
                {
                    #region 30分钟
                    //0-29 30-59
                    int offset = tickTime.DateTime.Minute % 30;
                    if (offset == 0)
                    {
                        return new DateTimeOffset(tickTime.DateTime.ClearSeconds(), tickTime.Offset);
                    }
                    else
                    {
                        return new DateTimeOffset(tickTime.DateTime.ClearSeconds().Subtract(TimeSpan.FromMinutes(offset)), tickTime.Offset);
                    }
                    #endregion
                }
                else if (truePeriod.NumberOfSecond >= 15 * 60)
                {
                    #region 15分钟
                    //0-14 15-29 30-44 45-59
                    int offset = tickTime.DateTime.Minute % 15;
                    if (offset == 0)
                    {
                        return new DateTimeOffset(tickTime.DateTime.ClearSeconds(), tickTime.Offset);
                    }
                    else
                    {
                        return new DateTimeOffset(tickTime.DateTime.ClearSeconds().Subtract(TimeSpan.FromMinutes(offset)), tickTime.Offset);
                    }
                    #endregion
                }
                else if (truePeriod.NumberOfSecond >= 5 * 60)
                {
                    #region 5分钟
                    //0-4 5-9 10-14 15-19 20-24 25-29 30-34 ...
                    int offset = tickTime.DateTime.Minute % 5;
                    if (offset == 0)
                    {
                        return new DateTimeOffset(tickTime.DateTime.ClearSeconds(), tickTime.Offset);
                    }
                    else
                    {
                        return new DateTimeOffset(tickTime.DateTime.ClearSeconds().Subtract(TimeSpan.FromMinutes(offset)), tickTime.Offset);
                    }
                    #endregion
                }
                else if (truePeriod.NumberOfSecond >= 60)
                {
                    #region 精确到分钟
                    return new DateTimeOffset(tickTime.DateTime.ClearSeconds(), tickTime.Offset);
                    #endregion
                }
                else if (truePeriod.NumberOfSecond >= 1 && truePeriod.NumberOfSecond < 60)
                {
                    #region 精确到秒
                    return new DateTimeOffset(tickTime.DateTime.TrimSeconds(), tickTime.Offset);
                    #endregion
                }
                #endregion
            }
            return tickTime;
        }

        public bool ExistsQueueCandle(DateTimeOffset tickTime, ref MT4Candle candle)
        {
            if (myCandleQue.Count() == 0)
            {
                return false;
            }
            else
            {
                if (Period is IntradayPeriodBase)
                {
                    #region IntradayPeriod
                    var truePeriod = (IntradayPeriodBase)Period;
                    if (truePeriod.NumberOfSecond >= 4 * 60 * 60)
                    {
                        #region H4
                        candle = myCandleQue.LastOrDefault(t =>
                        {
                            TimeSpan ts = tickTime.DateTime.ClearSeconds() - t.DateTime.DateTime.ClearSeconds();
                            double totalSeconds = ts.TotalSeconds;
                            return totalSeconds >= 0 && totalSeconds < 4 * 60 * 60;
                        }) as MT4Candle;
                        #endregion
                    }
                    else if (truePeriod.NumberOfSecond >= 2 * 60 * 60)
                    {
                        #region H2
                        candle = myCandleQue.LastOrDefault(t =>
                        {
                            TimeSpan ts = tickTime.DateTime.ClearSeconds() - t.DateTime.DateTime.ClearSeconds();
                            double totalSeconds = ts.TotalSeconds;
                            return totalSeconds >= 0 && totalSeconds < 2 * 60 * 60;
                        }) as MT4Candle;
                        #endregion
                    }
                    else if (truePeriod.NumberOfSecond >= 60 * 60)
                    {
                        #region H1
                        candle = myCandleQue.LastOrDefault(t =>
                        {
                            TimeSpan ts = tickTime.DateTime.ClearSeconds() - t.DateTime.DateTime.ClearSeconds();
                            double totalSeconds = ts.TotalSeconds;
                            return totalSeconds >= 0 && totalSeconds < 60 * 60;
                        }) as MT4Candle;
                        #endregion
                    }
                    else if (truePeriod.NumberOfSecond >= 30 * 60)
                    {
                        #region 30分钟
                        candle = myCandleQue.LastOrDefault(t =>
                        {
                            TimeSpan ts = tickTime.DateTime.ClearSeconds() - t.DateTime.DateTime.ClearSeconds();
                            double totalSeconds = ts.TotalSeconds;
                            return totalSeconds >= 0 && totalSeconds < 30 * 60;
                        }) as MT4Candle;
                        #endregion
                    }
                    else if (truePeriod.NumberOfSecond >= 15 * 60)
                    {
                        #region 15分钟
                        candle = myCandleQue.LastOrDefault(t =>
                        {
                            TimeSpan ts = tickTime.DateTime.ClearSeconds() - t.DateTime.DateTime.ClearSeconds();
                            double totalSeconds = ts.TotalSeconds;
                            return totalSeconds >= 0 && totalSeconds < 15 * 60;
                        }) as MT4Candle;
                        #endregion
                    }
                    else if (truePeriod.NumberOfSecond >= 5 * 60)
                    {
                        #region 5分钟
                        candle = myCandleQue.LastOrDefault(t =>
                        {
                            TimeSpan ts = tickTime.DateTime.ClearSeconds() - t.DateTime.DateTime.ClearSeconds();
                            double totalSeconds = ts.TotalSeconds;
                            return totalSeconds >= 0 && totalSeconds < 300;
                        }) as MT4Candle;
                        #endregion
                    }
                    else if (truePeriod.NumberOfSecond >= 60)
                    {
                        #region 精确到分钟
                        candle = myCandleQue.LastOrDefault(t => t.DateTime.DateTime.ClearSeconds() == tickTime.DateTime.ClearSeconds()) as MT4Candle;
                        #endregion
                    }
                    else if (truePeriod.NumberOfSecond >= 1 && truePeriod.NumberOfSecond < 60)
                    {
                        #region 精确到秒
                        candle = myCandleQue.LastOrDefault(t => t.DateTime.DateTime.TrimSeconds() == tickTime.DateTime.TrimSeconds()) as MT4Candle;
                        #endregion
                    }
                    #endregion
                }
                else if (Period is InterdayPeriodBase)
                {

                }
                return candle != null;
            }
        }

        public int TotalCandle()
        {
            return myCandleQue.Count;
        }

    }

    [DebuggerDisplay("Time:{DateTime} Open:{Open} Close:{Close} High:{High} Low:{Close} Volume:{Volume}")]
    public class MT4Candle : Candle
    {
        public MT4Candle(DateTimeOffset dateTime, decimal open, decimal high, decimal low, decimal close, decimal volume)
            : base(dateTime, open, high, low, close, volume)
        {

        }
    }

}
