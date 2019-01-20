using System;
using System.Collections.Generic;
using System.Drawing;
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
