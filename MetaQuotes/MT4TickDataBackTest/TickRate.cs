using System;

namespace MT4TickDataBackTest
{
    public struct TickRate
    {
        public DateTime TickTime;
        public double Bid;
        public double Ask;
        public long Volume;
    }
}
