using CSDistributeTransaction.Core.Tcc.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Abstract
{
    public abstract class DistributeTransactionBase<TKey> : IDistributeTransaction 
    {

        /// <summary>
        /// 事务id
        /// </summary>
        protected TKey TransactionId { get; set; }

        public DistributeTransactionBase(TKey key) 
        {
            this.TransactionId = key;
        }

        public abstract Task ExecuteAsync();
        public abstract Task CancelAsync();

    }
}
