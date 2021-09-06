using System;
using System.Collections.Generic;
using System.Text;

namespace CSDistributeTransaction.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DistributeTransactionStepAttribute:Attribute
    {
        public object State { get; private set; }
        public DistributeTransactionStepAttribute(object state)
        {
            this.State = state;
        }
    }
}
