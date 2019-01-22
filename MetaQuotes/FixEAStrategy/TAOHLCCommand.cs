namespace FixEAStrategy
{
    public class TAOHLCCommand : MTTACommand
    {
        public TAOHLCCommand(string symbol, string period, int barShift)
            : base(symbol, period, barShift)
        {

        }

        public override string SubCommandName => "OHLC";

        public override void JsonCallBack(string json)
        {
            Response = json;
            Context.ResetEvent.Set();
        }

    }
}
