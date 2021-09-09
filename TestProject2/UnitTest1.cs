using CSDistributeTransaction.Core.Tcc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TestProject2
{
    public class UnitTest1
    {

        [Fact]
        public async void Test1()
        {
            var list = new List<TccTransactionStep<object>>();
            list.Add(new TccTransactionStep1());
            list.Add(new TccTransactionStep2());

            ILoggerFactory loggerFactory= new LoggerFactory();
            
            TccTransaction trans = new TccTransaction(Guid.NewGuid(), list,CancellationToken.None, loggerFactory);

            await trans.ExecuteAsync();
            
        }
    }

    public class TccTransactionStep1 : TccTransactionStep<object>
    {
        public override Task Cancel()
        {
            Console.WriteLine("TccTransactionStep1 Cancel");
            return Task.CompletedTask;
        }

        public override Task Confirm()
        {
            Console.WriteLine("TccTransactionStep1 Confirm");
            return Task.CompletedTask;
        }

        public override Task Try()
        {
            Console.WriteLine("TccTransactionStep1 Try");
            return Task.CompletedTask;
        }
    }

    public class TccTransactionStep2 : TccTransactionStep<object>
    {
        public override Task Cancel()
        {
            Console.WriteLine("TccTransactionStep2 Cancel");
            return Task.CompletedTask;
        }

        public override Task Confirm()
        {
            throw new NotImplementedException();
            Console.WriteLine("TccTransactionStep2 Confirm");
            return Task.CompletedTask;
        }

        public override Task Try()
        {
            Console.WriteLine("TccTransactionStep2 Try");
            return Task.CompletedTask;
        }
    }

}

