using System;
using Sample.Enum;
using Sample.Store;

namespace Sample.Entity
{
    public class Order:BaseEntity
    {
        public OrderStatus Status { get; set; }
        public DateTime? PayDate { get; set; }
        public string Creator { get; set; }
    }
}