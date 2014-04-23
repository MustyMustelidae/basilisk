using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteLibrary.Messages.Values;

namespace RemoteLibrary.Serialization
{
    public static class SerializerHelper
    {
        public static SerializedRemoteInvocationValue ConvertObjectToInvocationValue(this IRemoteInterfaceSerializer serializer, Type valueType, object valueObject)
        {
            return new SerializedRemoteInvocationValue
            {
                ArgumentBytes = serializer.SerializeArgumentObject(valueObject),
                ArgumentType = valueType,
                IsBinaryType = serializer.IsBinaryType(valueType)
            };
        }

        public static IEnumerable<RemoteInvocationValue> ConvertObjectsToInvocationValues(
            this IRemoteInterfaceSerializer serializer,IEnumerable<object> objectEnumerable)
        {
            var objects = objectEnumerable as object[] ?? objectEnumerable.ToArray();
            var values = new List<RemoteInvocationValue>(objects.Length);
            values.AddRange(objects.Select(obj => new {obj, valueType = obj.GetType()})
                    .Select(value => serializer.ConvertObjectToInvocationValue(value.valueType, value.obj)));
            return values;
        }
    }
}
