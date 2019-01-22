using System;
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

        public static int OffSetPoints(this int digits, decimal priceLow, decimal priceHigh)
        {
            string appendFmt = "".PadLeft(digits, '#');
            string offSetStr = (priceHigh - priceLow).ToString("#." + appendFmt);
            return int.Parse(offSetStr.Replace(".", ""));
        }
    }
}
