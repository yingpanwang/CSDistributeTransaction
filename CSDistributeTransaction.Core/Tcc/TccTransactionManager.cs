using CSDistributeTransaction.Core.Exception;
using CSDistributeTransaction.Core.Option;
using CSDistributeTransaction.Core.Tcc.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Tcc
{
    public class TccTransactionManager
    {
        public TccTransactionOption Options { get; set; } = new TccTransactionOption();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private TccTransaction _currentTransaction;

        public TccTransactionManager(TccTransactionOption options,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider) 
        {
            Options = options;
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
        }


        public TccTransactionManager Create()
        {
            _currentTransaction = new TccTransaction(Guid.NewGuid(), new List<TccTransactionStep<object>>(), CancellationToken.None, _loggerFactory);

            return this;
        }

        public TccTransactionManager WithStep<TStep,TState>(TState state) where TStep:ITccTransactionStep 
        {
            var step = _serviceProvider.GetService(typeof(TStep));
            if (step == null)
                throw new System.Exception("Step不存在");

            var target = step as TccTransactionStep<TState>;

            if (target == null)
                throw new System.Exception("Step不存在");

            target.State = state;

            _currentTransaction.TccTransactionSteps = _currentTransaction.TccTransactionSteps.Append(target);
            return this;
        }

        public Task ExecuteAsync() 
        {
            return _currentTransaction.ExecuteAsync();
        }

    }
}
