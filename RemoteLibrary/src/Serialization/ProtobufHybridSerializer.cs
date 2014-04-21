using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ProtoBuf;
using ProtoBuf.Meta;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Values;

namespace RemoteLibrary.Serialization
{
    public class ProtobufHybridSerializer : IRemoteInterfaceSerializer
    {
        private const int MessageFieldNumber = 1;
        private const PrefixStyle MessagePrefixStyle = PrefixStyle.Fixed32;

        public RemoteInterfaceMessage DeserializeMessage(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return Serializer.DeserializeWithLengthPrefix<RemoteInterfaceMessage>(stream,
                MessagePrefixStyle, MessageFieldNumber);
        }

        public void SerializeMessage(Stream stream, RemoteInterfaceMessage interfaceMessage)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (interfaceMessage == null) throw new ArgumentNullException("interfaceMessage");
            Serializer.SerializeWithLengthPrefix(stream, interfaceMessage, MessagePrefixStyle);
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

        public SerializedRemoteInvocationValue SerializeObjectToInvocationValue(Type objectType, object valueObject)
        {
            return new SerializedRemoteInvocationValue
            {
                ArgumentBytes = SerializeArgumentObject(valueObject),
                ArgumentType = objectType,
                IsBinaryType = IsBinaryType(objectType)
            };
        }

        public object DeserializeArgumentToObject(SerializedRemoteInvocationValue argument)
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