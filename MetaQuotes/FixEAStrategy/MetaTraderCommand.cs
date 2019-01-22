using System;
using System.Collections.Generic;
using System.Text;

namespace FixEAStrategy
{
    public abstract class MetaTraderCommand : IMetaTraderCommand
    {
        public string CommandName { get; set; }

        /// <summary>
        /// 参数个数
        /// </summary>
        public int ParameterCount { get; set; }


        public abstract string GetParameterJson();

        public abstract void JsonCallBack(string json);

        public CommandExecContext Context { get; set; }

        public object Response { get; set; }

    }
    
}
