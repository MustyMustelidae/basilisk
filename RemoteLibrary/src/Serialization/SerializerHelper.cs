using System;
using System.Collections.Generic;
using System.Linq;
using RemoteLibrary.Messages.Values;

namespace RemoteLibrary.Serialization
{
    public static class SerializerHelper
    {
        public static SerializedRpcValue ConvertObjectToInvocationValue(this IRpcSerializer serializer,Type valueType, object valueObject)
        {
            return new SerializedRpcValue
            {
                ArgumentBytes = serializer.SerializeArgumentObject(valueObject),
                ArgumentType = valueType,
                IsBinaryType = serializer.IsBinaryType(valueType)
            };
        }

        public static IEnumerable<RpcValue> ConvertObjectsToInvocationValues(
            this IRpcSerializer serializer,IEnumerable<object> objectEnumerable)
        {
            var objects = objectEnumerable as object[] ?? objectEnumerable.ToArray();
            var values = new List<RpcValue>(objects.Length);
            values.AddRange(objects.Select(obj => new {obj, valueType = obj.GetType()})
                    .Select(value => serializer.ConvertObjectToInvocationValue(value.valueType, value.obj)));
            return values;
        }
    }
}
