using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchant.Models
{
    [Serializable]
    public class DummyPayRequest
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
    }

    [Serializable]
    public class DummyPayDetails
    {
        public string DummyPayId { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }

        public DummyPayDetails(string id, string transactionId, decimal amount)
        {
            this.DummyPayId = id;
            this.TransactionId = transactionId;
            this.Amount = amount;
        }
    }

    [Serializable]
    public class StatusRequest
    {
        public string TransactionId { get; set; }
        public string Receipt { get; set; }
    }

    [Serializable]
    public class StatusResponse
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }

        public StatusResponse(string id, string status, decimal amount, decimal amountPaid)
        {
            this.TransactionId = id;
            this.Status = status;
            this.Amount = amount;
            this.AmountPaid = amountPaid;
        }
    }

    [Serializable]
    public class NewTransactionRequest
    {
        public string RequestId { get; set; }
        public decimal Price { get; set; }
    }

    [Serializable]
    public class NewTransactionResponse
    {
        public string TransactionId { get; set; }

        public NewTransactionResponse(string id)
        {
            this.TransactionId = id;
        }
    }

    [Serializable]
    public class NewTransactionDetails
    {
        public string TransactionId { get; set; }
        public string RequestId { get; set; }
        public decimal Price { get; set; }

        public NewTransactionDetails(string id, string requestId, decimal price)
        {
            this.TransactionId = id;
            this.RequestId = requestId;
            this.Price = price;
        }
    }

    [Serializable]
    public class TransactionDetails
    {
        public string TransactionId { get; set; }
        public string Address { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Receipt { get; set; }

        public TransactionDetails(string id, string address, decimal amount, string currency, string receipt)
        {
            this.TransactionId = id;
            this.Address = address;
            this.Amount = amount;
            this.Currency = currency;
            this.Receipt = receipt;
        }
    }

    [Serializable]
    public class TransactionPayRequest
    {
        public string TransactionId { get; set; }
        public string Currency { get; set; }
    }

    [Serializable]
    public class TransactionRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public TransactionRequest(string id, string name, decimal price)
        {
            this.Id = id;
            this.Name = name;
            this.Price = price;
        }
    }
}
