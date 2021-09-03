using CSDistributeTransaction.Core.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Tcc
{
    public class TccTransaction:DistributeTransactionBase<Guid>
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _tryCancellationToken;
        private CancellationToken _confirmCancellationToken;
        public DistributeTransactionStatus Status { get; private set; }
        public IEnumerable<TccTransactionStep<object>> TccTransactionSteps { get; private set; }
        public TccTransaction(Guid key,IEnumerable<TccTransactionStep<object>> steps) 
            :base(key)
        {
            this.TccTransactionSteps = steps ?? new List<TccTransactionStep<object>>();
            
            _tryCancellationToken = new CancellationToken();
            _confirmCancellationToken = new CancellationToken();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_tryCancellationToken, _confirmCancellationToken);
        }

        public override async Task<bool> ExecuteAsync()
        {
            this.Status = DistributeTransactionStatus.Pending;

            var step = TccTransactionSteps.ToList();
            //_logger?.LogInformation($"事务开始:{this.TransactionId.ToString()}");
            var toeken = _cancellationTokenSource.Token;
            toeken.Register(async ()=> 
            {
                 this.Status = DistributeTransactionStatus.Canceled;
                 await CancelAsync();
            });

            _cancellationTokenSource.CancelAfter(3000);

            await TryExecute(step,_tryCancellationToken);
            
            this.Status = DistributeTransactionStatus.Pending;

            await ConfirmExecute(step,_confirmCancellationToken);
            
            this.Status = DistributeTransactionStatus.Confirmed;

            return true;
        }

        public override async Task CancelAsync()
        {
            //_logger?.LogInformation($"事务取消:{this.TransactionId.ToString()}");

            bool canceled = false;
            int retryCount = 0;
            int maxRetryCount = 5;

            while ((!canceled && retryCount < maxRetryCount )|| 
                this.Status != DistributeTransactionStatus.Manual)
            {
                try
                {
                    foreach (var cancelStep in TccTransactionSteps)
                    {
                        await cancelStep.Cancel();
                    }
                    
                    canceled = true;
                    break;
                    
                }
                catch (System.Exception)
                {
                    canceled = false;
                    if (retryCount == maxRetryCount)
                    {
                        this.Status = DistributeTransactionStatus.Manual;
                        return;
                    }
                    retryCount++;
                    Console.WriteLine($"重试:{retryCount}");
                    continue;
                }
                
            }
        }

        private async Task ConfirmExecute(IEnumerable<TccTransactionStep<object>> confirmSteps, CancellationToken cancellationToken)
        {
            foreach (var step in confirmSteps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await step.Confirm();
            }
        }
        private async Task TryExecute(IEnumerable<TccTransactionStep<object>> trySteps, CancellationToken cancellationToken)
        {
            foreach (var step in trySteps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (this.Status != DistributeTransactionStatus.Canceled)
                {
                    await step.Try();
                }
            }
        }


    }
}
