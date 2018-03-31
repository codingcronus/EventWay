using System;
using Dapper;

namespace EventWayCore.Infrastructure.Sqlite
{
    public class GuidAsBlobHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            var inVal = (byte[])value;
            var outVal = new[] { inVal[3], inVal[2], inVal[1], inVal[0], inVal[5], inVal[4], inVal[7], inVal[6], inVal[8], inVal[9], inVal[10], inVal[11], inVal[12], inVal[13], inVal[14], inVal[15] };
            return new Guid(outVal);
        }

        public override void SetValue(System.Data.IDbDataParameter parameter, Guid value)
        {
            var inVal = value.ToByteArray();
            var outVal = new[] 
                { inVal[3], inVal[2], inVal[1], inVal[0], inVal[5], inVal[4], inVal[7], inVal[6], inVal[8], inVal[9], inVal[10], inVal[11], inVal[12], inVal[13], inVal[14], inVal[15] };
            parameter.Value = outVal;
        }
    }
}