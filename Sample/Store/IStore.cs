using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sample.Store
{
    public interface IStore<TEntity> : IDisposable
    {
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> expression);

        Task Add(TEntity entity);

        Task Delete(string id);
        Task Delete(Predicate<TEntity> predicate);

        Task Update(string id, TEntity entity);
    }
}