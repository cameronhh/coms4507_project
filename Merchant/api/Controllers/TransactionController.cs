using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Merchant.Controllers
{
    [Serializable]
    public class TransactionRequest
    {
        public string Id { get; set; }
        public string name { get; set; }
        public decimal
    }

    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private ICacheClient getCacheClient()
        {
            // var mux = ConnectionMultiplexer.Connect(new ConfigurationOptions
            // {
            //     DefaultVersion = new Version(3, 0, 500),
            //     EndPoints = { { "localhost", 6379 } },
            //     AllowAdmin = true
            // });

            // return new StackExchangeRedisCacheClient(mux, new BinarySe)
            return ConnectionMultipler.Connect("localhost:6379");
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            var client = getCacheClient();
            var added = client.Add("transaction_" + value.Id.ToString(), value)
        }
    }
}
