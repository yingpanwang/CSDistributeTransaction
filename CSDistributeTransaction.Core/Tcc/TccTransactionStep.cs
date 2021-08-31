using CSDistributeTransaction.Core.Tcc.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Tcc
{
    public abstract class TccTransactionStep<TState>:ITccTransactionStep
    {
        public TState State { get; set; }

        public abstract Task Cancel();

        public abstract Task Confirm();

        public abstract Task Try();
    }
}
