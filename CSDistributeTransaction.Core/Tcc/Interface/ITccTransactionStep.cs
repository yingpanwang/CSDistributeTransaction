using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Tcc.Interface
{
    public interface ITccTransactionStep
    {
        Task Try();
        Task Confirm();
        Task Cancel();
    }
}
