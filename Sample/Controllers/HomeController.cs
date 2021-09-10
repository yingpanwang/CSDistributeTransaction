using CSDistributeTransaction.Core;
using CSDistributeTransaction.Core.Tcc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Entity;
using Sample.Services;
using Sample.Store;
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
        private readonly TccTransactionManager _tccTransactionManager;
        private readonly IStore<Stock> _store;
        public HomeController(TccTransactionManager tccTransactionManager,IStore<Stock> store) 
        {
            _tccTransactionManager = tccTransactionManager;
            _store = store;
        }


        [HttpGet]
        public async Task<IActionResult> Get([FromServices]ILoggerFactory loggerFactory,CancellationToken cancellationToken)
        {

            var list = new List<TccTransactionStep<object>>();
            //list.Add(new CreateOrderStep());
            //list.Add(new ReduceStockStep());

            //TccTransaction t = new TccTransaction(Guid.NewGuid(), list,cancellationToken,loggerFactory);
            //TccTransaction trans = new TccTransaction(Guid.NewGuid(), list, cancellationToken, loggerFactory);

            Parallel.For(0, 10,async (i) => 
            {
                Console.WriteLine("p:"+i);
                string orderId = Guid.NewGuid().ToString();

                var trans = _tccTransactionManager
                    .Create()
                    .WithStep<PlaceOrderStep, PlaceOrderStepState>(new PlaceOrderStepState()
                    {
                        CreatorId = Guid.NewGuid().ToString(),
                        OrderId = orderId
                    })
                    .WithStep<ReduceStockStep, ReduceStockState>(new ReduceStockState()
                    {
                        GoodsId = i.ToString(),
                        OrderId = orderId,
                        ReduceCount = 10
                    })
                    .ExecuteAsync();

                await trans;
            });

            for (int i = 0; i < 10; i++)
            {
                
                string orderId = Guid.NewGuid().ToString();

                var trans = _tccTransactionManager
                    .Create()
                    .WithStep<PlaceOrderStep, PlaceOrderStepState>(new PlaceOrderStepState()
                    {
                        CreatorId = Guid.NewGuid().ToString(),
                        OrderId = orderId
                    })
                    .WithStep<ReduceStockStep, ReduceStockState>(new ReduceStockState()
                    {
                        GoodsId = i.ToString(),
                        OrderId = orderId,
                        ReduceCount = 10
                    })
                    .ExecuteAsync();
                
                await trans;
            }

            var istore = _store as InMemeryStore<Stock>;

            foreach (var x in istore.Get(x=> 1==1))
            {
                decimal total = x.TotalStock;
                decimal frozen = x.Records.Where(x=>x.Status == Enum.StockRecordStatus.Frozen).Sum(x=>x.Count);
                decimal normal = x.Records.Where(x=>x.Status == Enum.StockRecordStatus.Normal).Sum(x=>x.Count);
                Console.WriteLine($"total :  {total} frozen :{frozen} normal:{normal}");
            }

            return Ok();
        }

    }

}
