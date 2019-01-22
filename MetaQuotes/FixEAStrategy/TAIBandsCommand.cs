using System;
using System.Collections.Generic;
using System.Text;

namespace FixEAStrategy
{
    public class TAIBandsCommand : MTTACommand
    {
        public TAIBandsCommand(string symbol, string period, int barShift) : base(symbol, period, barShift)
        {

        }

        public override string SubCommandName {
            get { return "iBands"; }
        }

        public override void JsonCallBack(string json)
        {
            /*
             * {"M1":[{"M" : "1283.24","L" : "1282.83","U" : "1283.66","shift" : 0},
                      { "M" : "1283.23","L" : "1282.77","U" : "1283.68","shift" : 1},
                      { "M" : "1283.20","L" : "1282.71","U" : "1283.70","shift" : 2},
                      { "M" : "1283.19","L" : "1282.65","U" : "1283.72","shift" : 3},
                      { "M" : "1283.15","L" : "1282.59","U" : "1283.72","shift" : 4}]}
           */
            Response = json;
            Context.ResetEvent.Set();
        }
    }
}
