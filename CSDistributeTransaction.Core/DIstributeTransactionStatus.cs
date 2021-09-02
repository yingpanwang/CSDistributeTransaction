using System;
using System.Collections.Generic;
using System.Text;

namespace CSDistributeTransaction.Core
{
    public enum DistributeTransactionStatus
    {
        /// <summary>
        /// 事务展开
        /// </summary>
        Pending,

        /// <summary>
        /// 已确认
        /// </summary>
        Confirmed,

        /// <summary>
        /// 已取消
        /// </summary>
        Canceled,

        /// <summary>
        /// 手动干预
        /// </summary>
        Manual
    }
}
