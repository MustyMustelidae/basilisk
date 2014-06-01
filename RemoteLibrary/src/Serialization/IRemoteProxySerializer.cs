using System;
using System.IO;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Values;

namespace RemoteLibrary.Serialization
{
    public interface IRpcSerializer
    {
        BaseRpcMessage DeserializeMessage(Stream stream);
        void SerializeMessage(Stream stream, BaseRpcMessage proxyInvocationMessage);
        byte[] SerializeArgumentObject(object argumentObject);
        SerializedRpcValue SerializeObjectToInvocationValue(Type objectType, object valueObject);
        object DeserializeArgumentToObject(SerializedRpcValue argument);
        bool IsBinaryType(Type type);
    }
}