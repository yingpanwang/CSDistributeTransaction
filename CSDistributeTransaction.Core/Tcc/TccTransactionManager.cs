using CSDistributeTransaction.Core.Exception;
using CSDistributeTransaction.Core.Option;
using CSDistributeTransaction.Core.Tcc.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Tcc
{
    public class TccTransactionManager
    {
        public TccTransaction StartNew(Guid tid) 
        {
            return new TccTransaction(tid,null);
        }
    }
}
