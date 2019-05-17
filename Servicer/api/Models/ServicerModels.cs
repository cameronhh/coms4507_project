using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Servicer.Models
{
    [Serializable]
    public class NewTaskRequest
    {
        public string Method { get; set; }
    }

    [Serializable]
    public class NewTaskResponse
    {
        public string RequestId { get; set; }
        public string TransactionId { get; set; }
        public NewTaskResponse(string id, string transactionId)
        {
            this.RequestId = id;
            this.TransactionId = transactionId;
        }
    }

    [Serializable]
    public class RunTaskRequest
    {
        public string RequestId { get; set; }
    }

    [Serializable]
    public class RunTaskResponse
    {
        public bool Result { get; set; }
        public RunTaskResponse(bool result)
        {
            this.Result = result;
        }
    }

    [Serializable]
    public class NewTransactionResponse
    {
        public string TransactionId { get; set; }
    }

    [Serializable]
    public class StatusResponse
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
    }

    [Serializable]
    public class RequestDetails
    {
        public string RequestId { get; set; }
        public string TransactionId { get; set; }
        public string Method { get; set; }

        public RequestDetails(string id, string transactionId, string method)
        {
            this.RequestId = id;
            this.TransactionId = transactionId;
            this.Method = method;
        }
    }
}
