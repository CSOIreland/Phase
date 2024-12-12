using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PhaseHarvester
{
    internal class Utility
    {
        private static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static string GetFloatArrayAsString(float[] input)
        {
            return '[' + String.Join(",", input.Select(p => p.ToString()).ToArray()) + ']';
        }

        public static string GetDateAsString(DateTime dateTime)
        {
            return dateTime.ToString("s");
        }
    }
}
