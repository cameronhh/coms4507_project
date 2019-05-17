using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Servicer.Models;

namespace Servicer.Util
{
    public static class HttpHelper
    {
        private const string MERCHANT_URI = "http://localhost:5000";

        private class JsonContent : StringContent
        {
            public JsonContent(object obj) :
                base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            { }
        }

        public static async Task<string> PostToMerchantAsync(string resourcePath, object content)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(MERCHANT_URI);

                var response = await client.PostAsync(resourcePath, new JsonContent(content));
                return await response.Content.ReadAsStringAsync();
            }
        }

    }
    public static class RandomGenerator
    {
        private static string GenerateString(int length)
        {
            const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random random = new Random();
            StringBuilder result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(charset[random.Next(charset.Length)]);
            }

            return result.ToString();
        }

        public static string GenerateToken()
        {
            return GenerateString(12);
        }

        public static DateTimeOffset GenerateTTL()
        {
            return DateTimeOffset.Now.AddHours(24);
        }
    }
}
