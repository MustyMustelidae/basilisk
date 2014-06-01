using System;
using System.IO;
using Moq;
using NUnit.Framework;
using ProtoBuf;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Properties;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Tests
{
    [TestFixture]
    public abstract class GenericRemoteInterfaceSerializerTests<T> where T : IRpcSerializer
    {
        protected abstract T GetNewInterfaceSerializer();

        [ProtoContract]
        protected class TestBaseRemoteProxyInvocationMessage : BaseRpcMessage
        {
            [UsedImplicitly]
            protected TestBaseRemoteProxyInvocationMessage()
            {
            }

            public TestBaseRemoteProxyInvocationMessage(Guid guid)
            {
                MessageGuid = guid;
            }
        }

        [Serializable]
        protected class BinarySerializableType
        {
            protected BinarySerializableType()
            {
            }
        }


        [ProtoContract]
        protected class TestRemoteInvocationValue : SerializedRpcValue
        {
            [UsedImplicitly]
            protected TestRemoteInvocationValue()
            {
            }

            public TestRemoteInvocationValue(Guid guid)
            {
                ArgumentBytes = guid.ToByteArray();
            }
        }

        [Test]
        public void CanSerializeThenDeserializeArgument()
        {
            var serializer = GetNewInterfaceSerializer();

            var guid = Guid.NewGuid();
            var serializedGuid = serializer.SerializeArgumentObject(guid);
            var argument = new SerializedRpcValue
            {
                ArgumentBytes = serializedGuid,
                ArgumentType = typeof (Guid)
            };


            var deserializedArgument = serializer.DeserializeArgumentToObject(argument);
            Assert.AreEqual(guid, deserializedArgument);
        }

        [Test]
        public void CanSerializeThenDeserializeMessage()
        {
            var serializer = GetNewInterfaceSerializer();
            using (var stream = new MemoryStream())
            {
                var guid = Guid.NewGuid();
                var testMessage = new TestBaseRemoteProxyInvocationMessage(guid);
                serializer.SerializeMessage(stream, testMessage);

                stream.Seek(0, SeekOrigin.Begin);

                var deserializedMessage = serializer.DeserializeMessage(stream);
                Assert.AreEqual(testMessage.MessageGuid, deserializedMessage.MessageGuid);
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void DeserializeArgumentToObjectThrowsExceptionOnNullArgument()
        {
            var serializer = GetNewInterfaceSerializer();
            serializer.DeserializeArgumentToObject(null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void DeserializeMessageThrowsExceptionOnNullStream()
        {
            var serializer = GetNewInterfaceSerializer();
            serializer.DeserializeMessage(null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void IsBinaryTypeThrowsExceptionOnNullType()
        {
            var serializer = GetNewInterfaceSerializer();
            serializer.IsBinaryType(null);
        }

        [Test]
        public void RecognizesBinaryType()
        {
            var serializer = GetNewInterfaceSerializer();
            var binaryType = serializer.IsBinaryType(typeof (BinarySerializableType));
            Assert.IsTrue(binaryType);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void SerializeArgumentObjectThrowsExceptionOnNullObject()
        {
            var serializer = GetNewInterfaceSerializer();
            serializer.SerializeArgumentObject(null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void SerializeMessageThrowsExceptionOnNullMessage()
        {
            var serializer = GetNewInterfaceSerializer();
            var streamMock = new Mock<Stream>();
            serializer.SerializeMessage(streamMock.Object, null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void SerializeMessageThrowsExceptionOnNullStream()
        {
            var serializer = GetNewInterfaceSerializer();
            var messageMock = new Mock<BaseRpcMessage>();
            serializer.SerializeMessage(null, messageMock.Object);
        }
    }
}