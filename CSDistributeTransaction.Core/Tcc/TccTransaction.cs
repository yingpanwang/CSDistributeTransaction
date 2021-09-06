using CSDistributeTransaction.Core.Abstract;
using CSDistributeTransaction.Core.Tcc.Interface;
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
        private readonly ILogger<TccTransaction> _logger;
        private CancellationToken _tryCancellationToken;
        private CancellationToken _confirmCancellationToken;
        public DistributeTransactionStatus Status { get; private set; }
        public IEnumerable<ITccTransactionStep> TccTransactionSteps { get; private set; }
        
        
        public TccTransaction(Guid key,IEnumerable<ITccTransactionStep> steps,
            ILoggerFactory loggerFactory) 
            :base(key)
        {
            this.TccTransactionSteps = steps ?? new List<TccTransactionStep<object>>();
            if(loggerFactory!=null)
                _logger = loggerFactory.CreateLogger<TccTransaction>();

            _tryCancellationToken = new CancellationToken();
            _confirmCancellationToken = new CancellationToken();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_tryCancellationToken, _confirmCancellationToken);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="steps"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="loggerFactory"></param>
        public TccTransaction(Guid key, IEnumerable<ITccTransactionStep> steps,
            CancellationToken cancellationToken,ILoggerFactory loggerFactory)
            : base(key)
        {
            this.TccTransactionSteps = steps ?? new List<ITccTransactionStep>();
            if(loggerFactory != null)
                _logger = loggerFactory.CreateLogger<TccTransaction>();

            _tryCancellationToken = new CancellationToken();
            _confirmCancellationToken = new CancellationToken();

            if (cancellationToken == null)
                cancellationToken = new CancellationToken();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_tryCancellationToken, _confirmCancellationToken,cancellationToken);
        }


        public override async Task<bool> ExecuteAsync()
        {
            using (_logger?.BeginScope("TransactionId:{0}", this.TransactionId))
            {
                _logger?.LogInformation($"事务开始:{this.TransactionId.ToString()}");

                var step = TccTransactionSteps.ToList();
                var toeken = _cancellationTokenSource.Token;
                toeken.Register(async () =>
                {
                    await CancelAsync();
                });

                _cancellationTokenSource.CancelAfter(10000);
                
                _logger?.LogInformation($"尝试执行事务:{this.TransactionId.ToString()}");
                await TryExecute(step, _tryCancellationToken);

                if (this.Status != DistributeTransactionStatus.Pending)
                    return false;

                _logger?.LogInformation($"确认执行事务:{this.TransactionId.ToString()}");
                await ConfirmExecute(step, _confirmCancellationToken);
                if (this.Status != DistributeTransactionStatus.Confirmed)
                    return false;

                _cancellationTokenSource.Dispose();
                return true;
            }
            
        }

        public override async Task CancelAsync()
        {
            _logger?.LogInformation($"事务取消:{this.TransactionId.ToString()}");

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
                    this.Status = DistributeTransactionStatus.Canceled;
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

        private async Task ConfirmExecute(IEnumerable<ITccTransactionStep> confirmSteps, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var step in confirmSteps)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await step.Confirm();
                }

                this.Status = DistributeTransactionStatus.Confirmed;
            }
            catch (System.Exception)
            {
                _cancellationTokenSource.Cancel();
                Status = DistributeTransactionStatus.Canceled;
                return;
            }
        }

        private async Task TryExecute(IEnumerable<ITccTransactionStep> trySteps, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var step in trySteps)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (this.Status != DistributeTransactionStatus.Canceled)
                    {
                        await step.Try();
                    }
                }

                this.Status = DistributeTransactionStatus.Pending;
            }
            catch (System.Exception)
            {
                _cancellationTokenSource.Cancel();
                Status = DistributeTransactionStatus.Canceled;
                return;
            }
        }
    }
}
