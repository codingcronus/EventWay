using System;
using System.Security.Cryptography;
using System.Text;

namespace EventWay.Infrastructure.CosmosDb
{
    internal static class PartitionKeyGenerator
    {
        internal static string Generate(Guid id, int noOfPartitions)
        {
            using (var md5 = MD5.Create())
            {
                var hashedValue = md5.ComputeHash(Encoding.UTF8.GetBytes(id.ToString()));
                var asInt = BitConverter.ToInt32(hashedValue, 0);
                asInt = asInt == int.MinValue ? asInt + 1 : asInt;

                return $"{Math.Abs(asInt) % noOfPartitions}";
            }
        }
    }
}
