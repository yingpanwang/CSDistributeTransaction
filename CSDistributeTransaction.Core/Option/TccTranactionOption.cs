using System;
using System.Collections.Generic;
using System.Text;

namespace CSDistributeTransaction.Core.Option
{
    public class TccTranactionOption
    {
        public int MaxRetryCount { get; set; }
        public long RetryInterval { get; set; }
    }
}
