using System;
using System.Collections.Generic;
using System.Linq;

namespace FixEAStrategy
{
    /// <summary>
    /// 有限容量，FIFO队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.Queue{T}" />
    public class LimitedQueue<T> : Queue<T>
    {
        private const int DefaultMaxItems = 10;

        private int maxItems;

        public LimitedQueue() : this(DefaultMaxItems) { }


        public LimitedQueue(int capacity) : base(capacity)
        {
            this.MaxItems = capacity;
        }

        public LimitedQueue(IEnumerable<T> list) : base(list)
        {
            this.MaxItems = Math.Max(list.Count(), DefaultMaxItems);
        }

        /// <summary>
        /// 获取或设置最多容量数
        /// </summary>
        public int MaxItems
        {
            get
            {
                return this.maxItems;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value");
                this.maxItems = value;
                limitedSize(value);
            }
        }

        void limitedSize(int n)
        {
            while (this.Count > n)
            {
                this.Dequeue();
            }
        }

        /// <summary>
        /// 过滤队列中的重复数据
        /// </summary>
        public IEnumerable<T> Distinct()
        {
            return ToArray().Distinct();
        }

        public new void Enqueue(T item)
        {
            limitedSize(this.maxItems - 1);
            base.Enqueue(item);
        }
    }
}
