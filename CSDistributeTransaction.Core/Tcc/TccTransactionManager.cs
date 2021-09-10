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

        public TccTransactionManager(TccTransactionOption options,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider) 
        {
            this.Options = options;
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
        }


        public TccTransaction Create()
        {
            return new TccTransaction(Guid.NewGuid(), new List<TccTransactionStep<object>>(), CancellationToken.None, _loggerFactory,_serviceProvider);
        }

    }
}
