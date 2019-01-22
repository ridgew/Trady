using System;
using System.Collections.Generic;
using System.Text;

namespace FixEAStrategy
{
    public class TAStochasticCommand : MTTACommand
    {
        public TAStochasticCommand(string symbol, string period, int barShift)
            : base(symbol, period, barShift)
        {

        }

        public override string SubCommandName => "iStochastic";

        public override void JsonCallBack(string json)
        {
            Response = json;
            Context.ResetEvent.Set();
        }
    }
}
