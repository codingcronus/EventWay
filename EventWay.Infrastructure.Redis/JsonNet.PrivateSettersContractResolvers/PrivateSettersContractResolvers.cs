using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace JsonNet.PrivateSettersContractResolvers
{
    public class RedisContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jProperty = base.CreateProperty(member, memberSerialization);

            if (!jProperty.Writable)
                jProperty.Writable = member.IsPropertyWithSetter();

            jProperty.ShouldSerialize = instance => true;

            if (jProperty.PropertyName == "AggregateId" ||
                jProperty.PropertyName == "AggregateType")
                jProperty.ShouldSerialize = instance => false;

            return jProperty;
        }
    }

    internal static class MemberInfoExtensions
    {
        internal static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;

            return property?.GetSetMethod(true) != null;
        }
    }
}