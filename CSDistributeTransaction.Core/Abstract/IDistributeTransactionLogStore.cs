using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Abstract
{
    public interface IDistributeTransactionLogStore
    {
        Task<TLog> Find<TLog>(string id);
        Task<IEnumerable<TLog>> GetList<TLog>(Expression<Func<bool,TLog>> expression);
        Task Add<TLog>(TLog log);
    }
}
