using System;
using Trady.Core;

namespace TickDataImporter
{
    public struct TickRate
    {
        public DateTime TickTime;
        public double Bid;
        public double Ask;
        public long Volume;
    }

    public static class Pack
    {
        public static MT4Candle ToCandle(this TickRate rate)
        {
            return new MT4Candle(new DateTimeOffset(rate.TickTime),
                Convert.ToDecimal(rate.Bid),
                Convert.ToDecimal(rate.Bid),
                Convert.ToDecimal(rate.Bid),
                Convert.ToDecimal(rate.Bid),
                Convert.ToDecimal(rate.Volume));
        }

        public static Candle MerginTickData(this Candle candle, TickRate tick)
        {
            decimal myPrice = Convert.ToDecimal(tick.Bid);
            candle.Close = myPrice;
            candle.High = Math.Max(candle.High, myPrice);
            candle.Low = Math.Min(candle.Low, myPrice);
            return candle;
        }
    }
}
