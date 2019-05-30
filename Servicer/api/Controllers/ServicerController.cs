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
            var postMerchantContent = new
            {
                price = REQUEST_PRICE
            };

            var postMerchantResponse = HttpHelper.PostToMerchantAsync("/api/transaction/new", postMerchantContent);
            var getChallengeResponse = HttpHelper.GetFromChallengeAsync("/ChallengeMe");

            NewTransactionResponse merchantResult = JsonConvert.DeserializeObject<NewTransactionResponse>(await postMerchantResponse);
            NewChallengeResponse challengeResult = JsonConvert.DeserializeObject<NewChallengeResponse>(await getChallengeResponse);

            var requestDetails = new RequestDetails(requestId, merchantResult.TransactionId, request.Method);

            var ttl = RandomGenerator.GenerateTTL();
            var added = client.Add(REQUEST_PREFIX + requestId, requestDetails, ttl);
            if (!added)
            {
                return BadRequest("Cache error");
            }

            var response = new NewTaskResponse(requestId, merchantResult.TransactionId, challengeResult.Header, challengeResult.Target);

            return Ok(response);
        }

        // POST api/servicer/runtask
        [HttpPost("runtask")]
        public async Task<IActionResult> RunTask([FromBody] RunTaskRequest request)
        {
            /* Handle an included challenge */
            if (request.Header != null)
            {

                var postChallengeContent = new
                {
                    header = request.Header,
                    target = request.Target
                };

                var postChallengeResponse = await HttpHelper.PostToChallengeAsync("/", postChallengeContent);
                ChallengeStatusResponse challengeResponse = JsonConvert.DeserializeObject<ChallengeStatusResponse>(postChallengeResponse);

                if (!challengeResponse.Access)
                {
                    return BadRequest("Invalid solution proivded");
                }

                var challengeExecutionResult = executeTask("executable");
                var challengeExecutionResponse = new RunTaskResponse(challengeExecutionResult);
                return Ok(challengeExecutionResponse);
            }

            var client = getCacheClient();

            RequestDetails requestDetails = client.Get<RequestDetails>(REQUEST_PREFIX + request.RequestId);
            if (requestDetails == null)
            {
                return NotFound("Invalid request details");
            }

            var postMerchantContent = new
            {
                transactionId = requestDetails.TransactionId
            };

            var postMerchantResponse = await HttpHelper.PostToMerchantAsync("/api/transaction/status", postMerchantContent);
            StatusResponse merchantResult = JsonConvert.DeserializeObject<StatusResponse>(postMerchantResponse);

            if (merchantResult.AmountPaid < merchantResult.Amount)
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
