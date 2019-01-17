using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trady.Core;
using Trady.Core.Infrastructure;
using Trady.Core.Period;

namespace MT4ZmqSingal
{

    public class ZmqCandle : IEnumerable<IOhlcv>
    {
        public ZmqCandle(string symbol, PeriodBase period)
        {
            _symbol = symbol;
            Period = period;
        }

        string _symbol = null;
        Queue<IOhlcv> myCandleQue = new Queue<IOhlcv>();

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
            MT4Candle candle = new MT4Candle(tickTime, bid, bid, ask, bid, 0);
            if (myCandleQue.Count() < 1)
            {
                myCandleQue.Enqueue(candle);
            }
            else
            {
                var last = (MT4Candle)myCandleQue.Last();
                TickRate tick = new TickRate { Ask = ask, Bid = bid, TickTime = tickTime.DateTime, Volume = lastVolume };
                if (Period is IntradayPeriodBase)
                {
                    #region IntradayPeriod
                    IntradayPeriodBase truePeriod = (IntradayPeriodBase)Period;
                    if (truePeriod.NumberOfSecond >= 60)
                    {
                        if (last.DateTime.DateTime.ClearSeconds() == tickTime.DateTime.ClearSeconds())
                        {
                            last.MerginTickData(tick);
                        }
                        else
                        {
                            myCandleQue.Enqueue(candle);
                        }
                    }
                    else if (truePeriod.NumberOfSecond >= 1 && truePeriod.NumberOfSecond < 60)
                    {
                        if (last.DateTime.DateTime.TrimSeconds() == tickTime.DateTime.TrimSeconds())
                        {
                            last.MerginTickData(tick);
                        }
                        else
                        {
                            myCandleQue.Enqueue(candle);
                        }
                    }
                    #endregion
                }
            }
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
