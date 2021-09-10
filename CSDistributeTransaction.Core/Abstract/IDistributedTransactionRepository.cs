using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Abstract
{
    public interface IDistributedTransactionRepository<TDistributedTransaction>
    {
        Task AddAsync(TDistributedTransaction transaction);
        Task UpdateAsync(TDistributedTransaction transaction);
        Task DeleteAsync<TTransaciontId>(TTransaciontId tid);

        Task<TDistributedTransaction> Find<TTransaciontId>(TTransaciontId tid);
        Task<IEnumerable<TDistributedTransaction>> Find(Expression<Func<bool, TDistributedTransaction>> expression);
        Task<IEnumerable<TDistributedTransaction>> FindAll();
    }
}
