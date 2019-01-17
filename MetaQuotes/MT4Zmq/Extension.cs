using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trady.Core;

namespace MT4ZmqSingal
{
    public struct TickRate
    {
        public DateTime TickTime;
        public decimal Bid;
        public decimal Ask;
        public decimal Volume;
    }

    public static class Extension
    {
        public static DateTime ClearSeconds(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
        }

        public static DateTime TrimSeconds(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
        }

        public static Candle MerginTickData(this Candle candle, TickRate tick)
        {
            decimal myPrice = Convert.ToDecimal(tick.Bid);
            candle.Close = myPrice;
            candle.High = Math.Max(candle.High, myPrice);
            candle.Low = Math.Min(candle.Low, myPrice);
            candle.Volume += 1;
            return candle;
        }
    }
}
