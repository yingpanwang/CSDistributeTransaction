using System;
using System.Collections.Generic;
using System.Text;

namespace CSDistributeTransaction.Core.Model
{
    public class TccTransactionLog<TKey>
    {
        public string Id { get; set; }

        public TKey TransactionId { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? FinishDate { get; set; }

        public long RetryCount { get; set; }
        public long MaxRetryCount { get; set; }
        public int RetryInterval { get; set; }
        public DistributeTransactionStatus Status { get; set; }

    }
}
