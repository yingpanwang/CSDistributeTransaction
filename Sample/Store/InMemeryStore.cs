using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Sample.Store
{
    public class InMemeryStore<TEntity> : IStore<TEntity>, IDisposable where TEntity : BaseEntity
    {
        private static List<TEntity> Context { get; set; } = new List<TEntity>();

        public IEnumerable<TEntity> Get(Expression<Func<TEntity,bool>> expression) 
        {
            return Context.Where(expression.Compile());
        }

        public Task Add(TEntity entity)
        {
            Context.Add(entity);
            return Task.CompletedTask;
        }

        public Task Delete(string id)
        {
            Context.RemoveAll(x => x.Id == id);
            return Task.CompletedTask;
        }

        public Task Delete(Predicate<TEntity> predicate)
        {
            Context.RemoveAll(predicate);
            return Task.CompletedTask;
        }

        public Task Update(string id, TEntity entity)
        {
            var fentity = Context.FirstOrDefault(x => x.Id == id);
            if (entity != null)
            {
                Context.Remove(fentity);
                Context.Add(entity);
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}