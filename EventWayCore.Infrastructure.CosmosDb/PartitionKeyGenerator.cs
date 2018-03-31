using System;

namespace EventWayCore.Infrastructure.CosmosDb
{
    public static class PartitionKeyGenerator
    {
        public static string Generate(Guid id, int noOfPartitions)
        {
            var asInt = id.GetHashCode();
            asInt = asInt == int.MinValue ? asInt + 1 : asInt;
            return $"{(Math.Abs(asInt) % noOfPartitions) + 1}";

            //using (var md5 = MD5.Create())
            //{
            //    var hashedValue = md5.ComputeHash(Encoding.UTF8.GetBytes(id.ToString()));
            //    var asInt = BitConverter.ToInt32(hashedValue, 0);
            //    asInt = asInt == int.MinValue ? asInt + 1 : asInt;

            //    return $"{Math.Abs(asInt) % noOfPartitions) + 1}";
            //}
        }
    }
}
