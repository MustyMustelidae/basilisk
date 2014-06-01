using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ProtoBuf;
using ProtoBuf.Meta;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Values;

namespace RemoteLibrary.Serialization
{
    public class ProtobufHybridSerializer : IRpcSerializer
    {
        private const int MessageFieldNumber = 1;
        private const PrefixStyle MessagePrefixStyle = PrefixStyle.Fixed32;

        public BaseRpcMessage DeserializeMessage(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return Serializer.DeserializeWithLengthPrefix<BaseRpcMessage>(stream,
                MessagePrefixStyle, MessageFieldNumber);
        }

        public void SerializeMessage(Stream stream, BaseRpcMessage proxyInvocationMessage)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (proxyInvocationMessage == null) throw new ArgumentNullException("proxyInvocationMessage");
            Serializer.SerializeWithLengthPrefix(stream, proxyInvocationMessage, MessagePrefixStyle);
        }

        public byte[] SerializeArgumentObject(object argumentObject)
        {
            if (argumentObject == null) throw new ArgumentNullException("argumentObject");
            byte[] objectBytes;
            using (var ms = new MemoryStream())
            {
                if (IsBinaryType(argumentObject.GetType()))
                {
                    var binarySerializer = new BinaryFormatter();
                    binarySerializer.Serialize(ms, argumentObject);
                }
                else
                {
                    Serializer.Serialize(ms, argumentObject);
                }
                ms.Flush();
                objectBytes = ms.ToArray();
            }
            return objectBytes;
        }

        public SerializedRpcValue SerializeObjectToInvocationValue(Type objectType, object valueObject)
        {
            return new SerializedRpcValue
            {
                ArgumentBytes = SerializeArgumentObject(valueObject),
                ArgumentType = objectType,
                IsBinaryType = IsBinaryType(objectType)
            };
        }

        public object DeserializeArgumentToObject(SerializedRpcValue argument)
        {
            if (argument == null) throw new ArgumentNullException("argument");

            object argumentObject;
            using (var ms = new MemoryStream(argument.ArgumentBytes))
            {
                if (IsBinaryType(argument.ArgumentType))
                {
                    var serializer = new BinaryFormatter();
                    argumentObject = serializer.Deserialize(ms);
                }
                else
                {
                    const string deserializeMethodName = "Deserialize";
                    var deserializeMethod = typeof (Serializer)
                        .GetMethod(deserializeMethodName);

                    var genericDeserializeMethod = deserializeMethod.MakeGenericMethod(argument.ArgumentType);

                    argumentObject = genericDeserializeMethod.Invoke(null, new object[] {ms});
                }
            }
            return argumentObject;
        }

        public bool IsBinaryType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return !RuntimeTypeModel.Default.CanSerialize(type);
        }
    }
}