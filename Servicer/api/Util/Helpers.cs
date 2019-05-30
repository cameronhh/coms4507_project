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
        private const string MERCHANT_URI = "http://localhost:7006";
        private const string CHALLENGE_URI = "http://localhost:5000";

        private class JsonContent : StringContent
        {
            public JsonContent(object obj) :
                base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            { }
        }

        private static async Task<string> GetFromUriAsync(string uri, string resourcePath)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);

                var response = await client.GetAsync(resourcePath);
                return await response.Content.ReadAsStringAsync();
            }
        }

        private static async Task<string> PostToUriAsync(string uri, string resourcePath, object content)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);

                var response = await client.PostAsync(resourcePath, new JsonContent(content));
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async Task<string> PostToMerchantAsync(string resourcePath, object content)
        {
            return await PostToUriAsync(MERCHANT_URI, resourcePath, content);
        }

        public static async Task<string> PostToChallengeAsync(string resourcePath, object content)
        {
            return await PostToUriAsync(CHALLENGE_URI, resourcePath, content);
        }

        public static async Task<string> GetFromChallengeAsync(string resourcePath)
        {
            return await GetFromUriAsync(CHALLENGE_URI, resourcePath);
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
