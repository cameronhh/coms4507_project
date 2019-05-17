using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchant.Util
{
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

        public static string GenerateAddress()
        {
            return GenerateString(18);
        }

        public static DateTimeOffset GenerateTTL()
        {
            return DateTimeOffset.Now.AddHours(24);
        }
    }
}
