using CSDistributeTransaction.Core;
using CSDistributeTransaction.Core.Tcc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        public HomeController(TccTransactionManager tccTransactionManager) 
        {
            _tccTransactionManager = tccTransactionManager;
        }


        [HttpGet]
        public async Task<IActionResult> Get([FromServices]ILoggerFactory loggerFactory,CancellationToken cancellationToken)
        {

            var list = new List<TccTransactionStep<object>>();
            //list.Add(new CreateOrderStep());
            //list.Add(new ReduceStockStep());

            //TccTransaction t = new TccTransaction(Guid.NewGuid(), list,cancellationToken,loggerFactory);
            //TccTransaction trans = new TccTransaction(Guid.NewGuid(), list, cancellationToken, loggerFactory);

            var trans = _tccTransactionManager
                .Create()
                .WithStep<ReduceStockStep, ReduceStockState>(new ReduceStockState()
                {
                    GoodsId = 1.ToString(),
                    OrderId = Guid.NewGuid().ToString(),
                    ReduceCount = 10
                })
                .ExecuteAsync();

            await trans;

            return Ok();
        }

    }

}
