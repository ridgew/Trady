using System;
using System.Collections.Generic;
using System.Text;

namespace FixEAStrategy
{
    public interface IMetaTraderCommand
    {
        int ParameterCount { get; set; }

        string CommandName { get; set; }

        string GetParameterJson();

        void JsonCallBack(string json);

        object Response { get; set; }

        CommandExecContext Context { get; set; }
    }
}
