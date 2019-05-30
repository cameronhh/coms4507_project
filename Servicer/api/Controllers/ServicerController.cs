using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Binary;
using Servicer.Models;
using Servicer.Util;

namespace Servicer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicerController : ControllerBase
    {
        private const string API_PREFIX = "servicer_";
        private const string REQUEST_PREFIX = API_PREFIX + "transaction_";
        private const decimal REQUEST_PRICE = 0.05m;

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

        // POST api/servicer/newtask
        [HttpPost("newtask")]
        public async Task<IActionResult> NewTask([FromBody] NewTaskRequest request)
        {
            var client = getCacheClient();

            var requestId = RandomGenerator.GenerateToken();
            var postContent = new
            {
                price = REQUEST_PRICE
            };

            var postResponse = await HttpHelper.PostToMerchantAsync("/api/transaction/new", postContent);
            NewTransactionResponse result = JsonConvert.DeserializeObject<NewTransactionResponse>(postResponse);

            var requestDetails = new RequestDetails(requestId, result.TransactionId, request.Method);

            var ttl = RandomGenerator.GenerateTTL();
            var added = client.Add(REQUEST_PREFIX + requestId, requestDetails, ttl);
            if (!added)
            {
                return BadRequest("Cache error");
            }

            var response = new NewTaskResponse(requestId, result.TransactionId);

            return Ok(response);
        }

        // POST api/servicer/runtask
        [HttpPost("runtask")]
        public async Task<IActionResult> RunTask([FromBody] RunTaskRequest request)
        {
            var client = getCacheClient();

            RequestDetails requestDetails = client.Get<RequestDetails>(REQUEST_PREFIX + request.RequestId);
            if (requestDetails == null)
            {
                return NotFound("Invalid request details");
            }

            var postContent = new
            {
                transactionId = requestDetails.TransactionId
            };

            var postResponse = await HttpHelper.PostToMerchantAsync("/api/transaction/status", postContent);
            StatusResponse result = JsonConvert.DeserializeObject<StatusResponse>(postResponse);

            if (result.AmountPaid < result.Amount)
            {
                return BadRequest("Payment not completed");
            }

            var executionResult = executeTask(requestDetails.Method);
            var response = new RunTaskResponse(executionResult);

            return Ok(response);
        }

        private bool executeTask(string method)
        {
            switch (method)
            {
                case "executable": return true;
                default: return false;
            }
        }
    }
}
