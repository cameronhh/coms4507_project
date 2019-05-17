using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Binary;
using Merchant.Models;
using Merchant.Util;

namespace Merchant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private const string TRANSACTION_PREFIX = "transaction_";
        private const string NEW_REQUEST_PREFIX = "request_";
        private const string DUMMY_PAY_PREFIX = "dummypay_";
        private const string CURRENCY_BTC = "BTC";

        private ICacheClient getCacheClient()
        {
            var mux = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                DefaultVersion = new Version(3, 0, 500),
                EndPoints = { { "localhost", 6379 } },
                AllowAdmin = true
            });

            return new StackExchangeRedisCacheClient(mux, new BinarySerializer());
        }

        // POST api/transaction/new
        [HttpPost("new")]
        public IActionResult Post([FromBody] NewTransactionRequest request)
        {
            var client = getCacheClient();

            var transactionId = RandomGenerator.GenerateToken();
            var newTransactionDetails = new NewTransactionDetails(transactionId, request.RequestId, request.Price);

            var ttl = RandomGenerator.GenerateTTL();
            var added = client.Add(NEW_REQUEST_PREFIX + transactionId, newTransactionDetails, ttl);
            if (!added)
            {
                return BadRequest("Cache error");
            }

            var response = new NewTransactionResponse(transactionId);

            return Ok(response);
        }

        // POST api/transaction/pay
        [HttpPost("pay")]
        public IActionResult Pay([FromBody] TransactionPayRequest request)
        {
            var client = getCacheClient();

            NewTransactionDetails newTransactionDetails = client.Get<NewTransactionDetails>(NEW_REQUEST_PREFIX + request.TransactionId);
            if (newTransactionDetails == null)
            {
                return NotFound("Invalid request details");
            }

            TransactionDetails existingTransaction = client.Get<TransactionDetails>(TRANSACTION_PREFIX + newTransactionDetails.TransactionId);
            if (existingTransaction != null)
            {
                return BadRequest("Transaction already exists");
            }

            var address = RandomGenerator.GenerateAddress();
            var receipt = RandomGenerator.GenerateToken();

            var transaction = new TransactionDetails(newTransactionDetails.TransactionId, address, newTransactionDetails.Price, CURRENCY_BTC, receipt);

            var ttl = RandomGenerator.GenerateTTL();
            var added = client.Add(TRANSACTION_PREFIX + transaction.TransactionId, transaction, ttl);
            if (!added)
            {
                return BadRequest("Cache error");
            }

            return Ok(transaction);
        }

        // POST api/transaction/dummypay
        [HttpPost("dummypay")]
        public IActionResult DummyPay([FromBody] DummyPayRequest request)
        {
            var client = getCacheClient();

            TransactionDetails transactionDetails = client.Get<TransactionDetails>(TRANSACTION_PREFIX + request.TransactionId);
            if (transactionDetails == null)
            {
                return NotFound("Invalid request details");
            }


            var dummyPayId = RandomGenerator.GenerateToken();
            var newDummyPay = new DummyPayDetails(dummyPayId, transactionDetails.TransactionId, request.Amount);

            var ttl = RandomGenerator.GenerateTTL();
            var added = client.Add(DUMMY_PAY_PREFIX + newDummyPay.TransactionId + dummyPayId, newDummyPay, ttl);
            if (!added)
            {
                return BadRequest("Cache error");
            }

            return Ok();
        }

        // POST api/transaction/status
        [HttpPost("status")]
        public IActionResult Status([FromBody] StatusRequest request)
        {
            var client = getCacheClient();

            TransactionDetails transactionDetails = client.Get<TransactionDetails>(TRANSACTION_PREFIX + request.TransactionId);
            if (transactionDetails == null)
            {
                return NotFound("Invalid request details");
            }

            if (request.Receipt != transactionDetails.Receipt)
            {
                return BadRequest("Invalid receipt");
            }

            var totalPayment = GetCurrentlyPaidAmount(transactionDetails.TransactionId);

            var status = "incomplete";
            if (totalPayment >= transactionDetails.Amount)
            {
                status = "complete";
            }

            var response = new StatusResponse(transactionDetails.TransactionId, status, transactionDetails.Amount, totalPayment);

            return Ok(response);
        }

        private decimal GetCurrentlyPaidAmount(string transactionId)
        {
            var client = getCacheClient();

            var dummyPayKeys = client.SearchKeys(DUMMY_PAY_PREFIX + transactionId + "*");
            var dummyPays = client.GetAll<DummyPayDetails>(dummyPayKeys);

            decimal result = 0;
            if (dummyPays != null)
            {
                foreach (KeyValuePair<string, DummyPayDetails> dummyPayPair in dummyPays)
                {
                    result += dummyPayPair.Value.Amount;
                }
            }

            return result;
        }
    }
}
