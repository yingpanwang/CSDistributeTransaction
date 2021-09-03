using CSDistributeTransaction.Core;
using CSDistributeTransaction.Core.Tcc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> Get()
        {

            var list = new List<TccTransactionStep<object>>();
            list.Add(new CreateOrderStep());
            list.Add(new ReduceStockStep());
            
            TccTransaction t = new TccTransaction(Guid.NewGuid(), list);

            await t.ExecuteAsync();
            
            return Ok();
        }

    }

    [DistributeTransactionStep]
    public class PayStep 
    {
    
    }

    public class ReduceStockStep : TccTransactionStep<object>
    {
        public override Task Cancel()
        {
            Console.WriteLine("取消添加库存");
            return Task.CompletedTask;
        }

        public override Task Confirm()
        {
            Console.WriteLine("确认添加库存");
            return Task.CompletedTask;
        }

        public override Task Try()
        {
            Console.WriteLine("尝试添加库存");

            throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }

    public class CreateOrderStep : TccTransactionStep<object>
    {
        public override Task Cancel()
        {
            Console.WriteLine("取消添加订单");
            return Task.CompletedTask;
        }

        public override Task Confirm()
        {
            Console.WriteLine("确认添加订单");
            return Task.CompletedTask;
        }

        public override Task Try()
        {
            Console.WriteLine("尝试添加订单");
            return Task.CompletedTask;
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
