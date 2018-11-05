using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trady.Core;
using System.Diagnostics;

namespace TickDataImporter
{
    [DebuggerDisplay("Time:{DateTime} Open:{Open} Close:{Close} High:{High} Low:{Close} Volume:{Volume}")]
    public class MT4Candle : Candle
    {
        public MT4Candle(DateTimeOffset dateTime, decimal open, decimal high, decimal low, decimal close, decimal volume)
            : base(dateTime, open, high, low, close, volume)
        {

        }
    }
}
