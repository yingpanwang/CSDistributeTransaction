using Sample.Enum;
using Sample.Store;

namespace Sample.Entity
{
    public class StockRecord : BaseEntity
    {
        public StockRecordType Type { get; set; }
        public decimal Count { get; set; }
        public string OrderId { get; set; }
        public StockRecordStatus Status { get; set; }
    }
}