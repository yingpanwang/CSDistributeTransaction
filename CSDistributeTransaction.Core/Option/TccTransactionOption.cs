using System;
using System.Collections.Generic;
using System.Text;

namespace CSDistributeTransaction.Core.Option
{
    public class TccTransactionOption
    {
        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// 重试间隔
        /// </summary>
        public long RetryInterval { get; set; }
    }
}
