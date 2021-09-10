using CSDistributeTransaction.Core.Abstract;
using CSDistributeTransaction.Core.Tcc.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Tcc
{
    public class TccTransaction:DistributeTransactionBase<Guid>
    {
        // 取消任务主令牌源
        private readonly CancellationTokenSource _mainCancellationTokenSource ;
        
        // “try阶段”任务取消令牌源
        private readonly CancellationTokenSource _tryCancellationTokenSource = default!;
        
        // “confirm阶段"任务取消令牌
        private readonly CancellationTokenSource _confirmCancellationTokenSource = default!;
        
        // 日志
        private readonly ILogger<TccTransaction> _logger = default!;

        private readonly IServiceProvider _serviceProvider;
        
        /// <summary>
        /// 事务状态
        /// </summary>
        public DistributeTransactionStatus Status { get; private set; }
        
        /// <summary>
        /// 事务步骤
        /// </summary>
        public IEnumerable<ITccTransactionStep> TccTransactionSteps { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key">事务id</param>
        /// <param name="steps">事务执行步骤</param>
        /// <param name="cancellationToken">外部取消令牌</param>
        /// <param name="loggerFactory">日志</param>
        public TccTransaction(Guid key, IEnumerable<ITccTransactionStep> steps,
            CancellationToken cancellationToken,ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider)
            : base(key)
        {
            // 初始化事务步骤列表
            this.TccTransactionSteps = steps ?? new List<ITccTransactionStep>();
            _serviceProvider = serviceProvider;
            // 初始化日志
            _logger = loggerFactory.CreateLogger<TccTransaction>();
            
            // 初始化取消令牌关联
            _tryCancellationTokenSource = new CancellationTokenSource();
            _confirmCancellationTokenSource = new CancellationTokenSource();
            _mainCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_tryCancellationTokenSource.Token,_confirmCancellationTokenSource.Token,cancellationToken);
        }


        public TccTransaction WithStep<TStep, TState>(TState state) where TStep : ITccTransactionStep
        {
            var step = _serviceProvider.GetService(typeof(TStep));
            if (step == null)
                throw new System.Exception("Step不存在");

            var target = step as TccTransactionStep<TState>;

            if (target == null)
                throw new System.Exception("Step不存在");

            target.State = state;

            this.TccTransactionSteps = this.TccTransactionSteps.Append(target);
            return this;
        }


        /// <summary>
        /// 事务执行
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> ExecuteAsync()
        {
            if (TccTransactionSteps.Count() == 0)
                throw new ArgumentException("没有需要执行的Step!");

            using (_logger?.BeginScope("TransactionId:{0}", this.TransactionId))
            {
                _logger?.LogInformation($"事务开始:{this.TransactionId.ToString()}");

                var step = TccTransactionSteps.ToList();
                var token = _mainCancellationTokenSource.Token;
                token.Register(async () =>
                {
                    await CancelAsync();
                });
                
                //TODO:添加TransactionOptions类将配置提取到类中
                
                _mainCancellationTokenSource.CancelAfter(10000);
                
                _logger?.LogInformation($"尝试执行事务:{this.TransactionId.ToString()}");
                await TryExecute(step, _tryCancellationTokenSource.Token);
                
                if (this.Status != DistributeTransactionStatus.Pending)
                    return false;

                _logger?.LogInformation($"确认执行事务:{this.TransactionId.ToString()}");
                await ConfirmExecute(step, _confirmCancellationTokenSource.Token);
                if (this.Status != DistributeTransactionStatus.Confirmed)
                    return false;

                _mainCancellationTokenSource.Dispose();
                return true;
            }
            
        }
        
        /// <summary>
        /// 取消事务（执行终止）
        /// </summary>
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
        
        /// <summary>
        /// 执行confirm阶段
        /// </summary>
        /// <param name="confirmSteps">confirm步骤</param>
        /// <param name="cancellationToken">取消令牌</param>
        private async Task ConfirmExecute(IEnumerable<ITccTransactionStep> confirmSteps, CancellationToken cancellationToken)
        {
            try
            {
                // TODO:重试策略
                // 遍历执行confirm阶段
                foreach (var step in confirmSteps)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await step.Confirm();
                }

                this.Status = DistributeTransactionStatus.Confirmed;
            }
            catch (System.Exception)
            {
                // 调用Cancel，通过Cancel监听 执行事务CancelAsync方法
                _mainCancellationTokenSource.Cancel();
                Status = DistributeTransactionStatus.Canceled;
                return;
            }
        }

        /// <summary>
        /// 执行try阶段
        /// </summary>
        /// <param name="trySteps"></param>
        /// <param name="cancellationToken"></param>
        private async Task TryExecute(IEnumerable<ITccTransactionStep> trySteps, CancellationToken cancellationToken)
        {
            try
            {
                //TODO:重试策略
                // 遍历执行各步骤try
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
                // 调用Cancel，通过Cancel监听 执行事务CancelAsync方法
                _mainCancellationTokenSource.Cancel();
                Status = DistributeTransactionStatus.Canceled;
                return;
            }
        }
        
    }
}
