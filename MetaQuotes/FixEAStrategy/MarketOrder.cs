using System;

namespace FixEAStrategy
{
    [Serializable]
    public struct MarketOrder
    {
        int OrderTicket;

        /*
         OP_BUY - 0 - buy order,
         OP_SELL - 1 - sell order,
         OP_BUYLIMIT -2- buy limit pending order,
         OP_SELLLIMIT -3-  sell limit pending order,
         OP_BUYSTOP -4- buy stop pending order,
         OP_SELLSTOP -5-  sell stop pending order.
        */
        public MTOrderType OrderType;

        public double OpenPrice;

        public double ClosePrice;

        /// <summary>
        /// 库存
        /// </summary>
        public double OrderSwap;

        /// <summary>
        /// 佣金
        /// </summary>
        public double OrderCommission;

        public string Symbol;

        public DateTimeOffset? OpenTime;

        public DateTimeOffset? CloseTime;

        public DateTimeOffset? OrderExpiration;

        int OrderMagicNumber;

        string OrderComment;
    }

    public enum MTOrderType : int
    {
        /// <summary>
        /// (0) buy order 
        /// </summary>
        OP_BUY = 0,

        /// <summary>
        /// (1) sell order
        /// </summary>
        OP_SELL = 1,

        /// <summary>
        /// (2) buy limit pending order
        /// </summary>
        OP_BUYLIMIT = 2,

        /// <summary>
        /// (3) sell limit pending order
        /// </summary>
        OP_SELLLIMIT = 3,

        /// <summary>
        /// (4) buy stop pending order
        /// </summary>
        OP_BUYSTOP = 4,

        /// <summary>
        /// (5) sell stop pending order
        /// </summary>
        OP_SELLSTOP = 5
    }

}
