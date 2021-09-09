using System;
using System.Collections.Generic;
using System.Text;

namespace CSDistributeTransaction.Core
{
    public class ExecuteResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public ExecuteResult(bool isSuccess,string message)
        {
            this.IsSuccess = isSuccess;
            this.Message = message == null ?string.Empty:message;
        }
    }
}
