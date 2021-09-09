using Sample.Store;
using System.Collections.Generic;

namespace Sample.Entity
{
    public class Stock : BaseEntity
    {
        public string GoodsId { get; set; }
        public decimal TotalStock { get; set; }

        public virtual IList<StockRecord> Records { get; set; }
    }
}