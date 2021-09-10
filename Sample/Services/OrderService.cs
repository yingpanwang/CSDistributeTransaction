using CSDistributeTransaction.Core.Tcc;
using CSDistributeTransaction.Core.Tcc.Interface;
using Sample.Entity;
using Sample.Store;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.Services
{
    public class OrderService
    {
        private readonly IStore<Order> _store;
        public OrderService(IStore<Order> store)
        {
            _store = store;
        }

    }

    public class PlaceOrderStepState 
    {
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string CreatorId { get; set; } 
    }

    public class PlaceOrderStep : TccTransactionStep<PlaceOrderStepState>
    {
        private readonly IStore<Order> _store;
        public PlaceOrderStep(IStore<Order> store) 
        {
            _store = store;
        }

        public override async Task Cancel()
        {
            await _store.Delete(x=>x.Id == State.OrderId);
        }

        public override async Task Confirm()
        {
            var frozenOrder = _store.Get(x=>x.Id == State.OrderId).FirstOrDefault();
            if (frozenOrder == null)
            {
                throw new InvalidOperationException("获取订单信息shibai");
            }

            frozenOrder.Status = Enum.OrderStatus.Created;
            await _store.Update(frozenOrder.Id, frozenOrder);
        }

        public override async Task Try()
        {
            var order = new Order()
            {
                Id = State.OrderId,
                CreateDate = DateTime.Now,
                Creator = State.CreatorId,
                Status = Enum.OrderStatus.Frozen
            };

           await _store.Add(order);
        }
    }
}