using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FixEAStrategy
{
    public class CommandExecContext : IDisposable
    {
        public CommandExecContext()
        {
            ResetEvent = new ManualResetEvent(false);
        }

        public ManualResetEvent ResetEvent { get; set; }

        public void Dispose()
        {
            if (ResetEvent != null)
                ResetEvent.Dispose();
        }
    }
}
