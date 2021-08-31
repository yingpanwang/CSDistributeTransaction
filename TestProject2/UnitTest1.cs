using CSDistributeTransaction.Core.Tcc;
using System;
using System.Threading.Tasks;
using Xunit;

namespace TestProject2
{
    public class UnitTest1
    {

        [Fact]
        public async void Test1()
        {
            TccTransactionManager manager = new TccTransactionManager();

            var tid = Guid.NewGuid().ToString();

            await manager
               .Start(tid)
               .With<TccTransactionStep1, object>(new { Name = "1111" })
               .With<TccTransactionStep2, object>(new { Name = "2222" })
               .ExecuteAsync();

            Assert.True(true);
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

