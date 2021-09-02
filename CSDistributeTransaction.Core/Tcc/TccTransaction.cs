using CSDistributeTransaction.Core.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Tcc
{
    public class TccTransaction:DistributeTransactionBase<Guid>
    {
        public DistributeTransactionStatus Status { get; private set; }
        public IEnumerable<TccTransactionStep<object>> TccTransactionSteps { get; private set; }
        public TccTransaction(Guid key,IEnumerable<TccTransactionStep<object>> steps):base(key)
        {
            this.TccTransactionSteps = steps;
        }

        public override async Task ExecuteAsync()
        {
            this.Status = DistributeTransactionStatus.Pending;

            var step = TccTransactionSteps.ToList();
            
            await TryExecute(step);
            await ConfirmExecute(step);

        }

        public override async Task CancelAsync()
        {
            await ExecuteStep(async (steps) => 
            {
                foreach (var step in steps)
                {
                    await step.Cancel();
                }
            });

            this.Status = DistributeTransactionStatus.Canceled;
        }

        private async Task ConfirmExecute(IEnumerable<TccTransactionStep<object>> confirmSteps) 
        {
            try
            {
                List<Task> executeList = new List<Task>();
                foreach (var step in confirmSteps)
                {
                    executeList.Add(step.Confirm());
                }
                await Task.WhenAll(executeList);

                this.Status = DistributeTransactionStatus.Confirmed;
            }
            catch (System.Exception)
            {
                await this.CancelAsync();
            }
        }
        private async Task TryExecute(IEnumerable<TccTransactionStep<object>> trySteps) 
        {
            try
            {
                List<Task> executeList = new List<Task>();
                foreach (var step in trySteps)
                {
                    executeList.Add(step.Try());
                }
                await Task.WhenAll(executeList);
            }
            catch (System.Exception)
            {
                await this.CancelAsync();
            }
        }

        private async Task ExecuteStep(Action<IEnumerable<TccTransactionStep<object>>> action) 
        {
            try
            {
                action(TccTransactionSteps);
            }
            catch (System.Exception)
            {
                await this.CancelAsync();
            }
        }
    }
}
