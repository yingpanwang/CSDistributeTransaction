using System;
using System.Linq;
using System.Collections.Generic;
using Sample.Store;
using Sample.Entity;
using CSDistributeTransaction.Core.Abstract;
using CSDistributeTransaction.Core.Tcc.Interface;
using System.Threading.Tasks;
using CSDistributeTransaction.Core.Tcc;
using System.Security.Cryptography.X509Certificates;

namespace Sample.Services
{
    public class StockService
    {
        private readonly IStore<Stock> _store;
        public StockService(IStore<Stock> store)
        {
            _store = store;
        }
    }

    public class ReduceStockState 
    {
        public string GoodsId { get; set; }
        public string OrderId { get; set; }
        public decimal ReduceCount { get; set; }
    }
    public class ReduceStockStep : TccTransactionStep<ReduceStockState>
    {
        private readonly IStore<Stock> _store;
        public ReduceStockStep(IStore<Stock> store)
        {
            _store = store;
        }
        public override async Task Cancel()
        {
            var stock = _store.Get(x=>x.GoodsId == State.GoodsId).FirstOrDefault();
            if (stock == null)
                throw new Exception("?????????");

            var record = stock.Records.FirstOrDefault(x=>x.OrderId == State.OrderId);
            stock.Records.Remove(record);

            await _store.Update(stock.Id,stock);
        }

        public override async Task Confirm()
        {
            var stock = _store.Get(x => x.GoodsId == State.GoodsId).FirstOrDefault();
            if (stock == null)
                throw new Exception("库存不存在");

            stock.TotalStock -= State.ReduceCount;
            var record = stock.Records.FirstOrDefault(x=>x.OrderId == State.OrderId && 
                                            x.Status == Enum.StockRecordStatus.Frozen &&
                                            x.Type == Enum.StockRecordType.Reduce);
            if (record == null)
                throw new Exception("获取信息错误");


            record.Status = Enum.StockRecordStatus.Normal;

            await _store.Update(stock.Id, stock);
        }

        public override async Task Try()
        {
            var stock = _store.Get(x=>x.GoodsId == State.GoodsId).FirstOrDefault();
            if (stock == null)
                throw new Exception("商品不存在");

            var frozenStock = stock.Records
                .Where(x=>x.Status == Enum.StockRecordStatus.Frozen && x.Type == Enum.StockRecordType.Reduce)
                .Sum(x=>x.Count);

            if (stock.TotalStock <= frozenStock || (stock.TotalStock - frozenStock - State.ReduceCount < 0))
                throw new Exception("库存不足");
            
            var records = stock.Records;
            records.Add(new StockRecord()
            {
                Count = State.ReduceCount,
                CreateDate = DateTime.Now,
                Id = Guid.NewGuid().ToString(),
                Status = Enum.StockRecordStatus.Frozen,
                Type = Enum.StockRecordType.Reduce,
                OrderId = State.OrderId
            });

            await _store.Update(stock.Id, stock);
        }
    }
}