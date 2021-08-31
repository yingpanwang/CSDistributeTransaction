using CSDistributeTransaction.Core.Exception;
using CSDistributeTransaction.Core.Option;
using CSDistributeTransaction.Core.Tcc.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSDistributeTransaction.Core.Tcc
{
    public class TccTransactionManager
    {
        private string _transactionId;
        private TccTranactionOption _tccTranactionOption;

        private List<ITccTransactionStep> _tccTransactionSteps = new List<ITccTransactionStep> ();

        public TccTransactionManager Start(string transactionId,TccTranactionOption option = null) 
        {
            _transactionId = transactionId;
            _tccTranactionOption = option == null ? new TccTranactionOption() : option;

            return this;
        }

        public TccTransactionManager With<TTransactionStep, TState>(TState state) where TTransactionStep : ITccTransactionStep
        {
            Type transType = typeof(TTransactionStep);

            var step = Activator.CreateInstance(transType);
            if (step == null)
                throw new InvalidTccTransactionStepException();

            _tccTransactionSteps.Add((ITccTransactionStep)step);

            return this;
        }


        public Task ExecuteAsync() 
        {
            foreach (var step in _tccTransactionSteps)
            {
                step.Try();
            }

            foreach (var step in _tccTransactionSteps)
            {
                step.Confirm();
            }

            foreach (var step in _tccTransactionSteps)
            {
                step.Cancel();
            }
            return Task.FromResult(true);
        }

    }
}
