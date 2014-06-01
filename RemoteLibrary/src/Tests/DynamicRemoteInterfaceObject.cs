using System;
using System.IO;
using System.Threading.Tasks;
using ImpromptuInterface;
using Moq;
using NUnit.Framework;
using RemoteLibrary.Channels;
using RemoteLibrary.InterfaceProviders;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Results;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;
using RemoteLibrary.Util.Cached;
using Shared;

namespace RemoteLibrary.Tests
{
    [TestFixture]
    public class DynamicRemoteInterfaceObjectTests
    {
        public interface ITestInterface
        {
            object ReturnValue(object val);
            void NonReturningMethod();
            void ExceptionThrowingMethod();
        }
        [Serializable]
        private class TestException : Exception
        {
        }

        private readonly Type _testType = typeof (ITestInterface);

        private static Guid TestGuid
        {
            get { return Guid.Empty; }
        }

        private static byte[] TestBytes
        {
            get { return TestGuid.ToByteArray(); }
        }

        private ITestInterface GenerateTestClassProxyClass(BaseRpcMessage expectedMessage, object expectedRawValue,
            SerializedRpcValue expectedValue, out Mock<IRpcSerializer> serializerMock,
            out Mock<IRpcChannel> channelMock)
        {
            serializerMock = new Mock<IRpcSerializer>();
            serializerMock.Setup(serializer => serializer.SerializeArgumentObject(It.IsAny<object>()))
                .Returns(TestBytes);

            serializerMock.Setup(
                serializer => serializer.SerializeMessage(It.IsAny<Stream>(), It.IsAny<BaseRpcMessage>()));

            serializerMock.Setup(
                serializer => serializer.SerializeObjectToInvocationValue(It.IsAny<Type>(), It.IsAny<object>()))
                .Returns(expectedValue);

            serializerMock.Setup(
                serializer => serializer.DeserializeArgumentToObject(It.IsAny<SerializedRpcValue>()))
                .Returns(expectedRawValue);

            serializerMock.Setup(
                serializer => serializer.DeserializeMessage(It.IsAny<Stream>()))
                .Returns(expectedMessage);

            channelMock = new Mock<IRpcChannel>();

            channelMock.Setup(channel => channel.SendMessage(It.IsAny<BaseRpcMessage>()))
                .Returns(Task.FromResult(expectedMessage));

            var infoResolverMock = new Mock<ICachedTypeResolver>();

            var guidProviderMock = new Mock<IGuidProvider>();
            guidProviderMock.Setup(provider => provider.GetNewGuid()).Returns(TestGuid);
            var interfaceObject = new ChannelRpcDynamicObject(channelMock.Object, infoResolverMock.Object,
                _testType,
                serializerMock.Object, guidProviderMock.Object);

            var proxy = interfaceObject.ActLike<ITestInterface>();
            return proxy;
        }

        [TestCase("Test")]
        [TestCase(9)]
        public void CanInvokeMethodWithArgsAndParams(object expectedValue)
        {
            var expectedInvocationValue = new SerializedRpcValue
            {
                ArgumentBytes = TestBytes,
                ArgumentType = expectedValue.GetType(),
                IsBinaryType = true
            };
            var expectedResult = new NonNullRemoteProxyInvocationResult
            {
                MessageGuid = TestGuid,
                ReturnValue = expectedInvocationValue
            };
            Mock<IRpcSerializer> serializerMock;
            Mock<IRpcChannel> channelMock;
            var proxy = GenerateTestClassProxyClass(expectedResult, expectedValue, expectedInvocationValue,
                out serializerMock,
                out channelMock);

            var returnedValue = proxy.ReturnValue(expectedValue);
            Assert.AreEqual(expectedValue, returnedValue);
        }

        public void CanInvokeMethodWithArgsAndParamsThatAreExceptions()
        {
            var expectedValue = new TestException();
            var expectedInvocationValue = new SerializedRpcValue
            {
                ArgumentBytes = TestBytes,
                ArgumentType = expectedValue.GetType(),
                IsBinaryType = true
            };
            var expectedResult = new NonNullRemoteProxyInvocationResult
            {
                MessageGuid = TestGuid,
                ReturnValue = expectedInvocationValue
            };
            Mock<IRpcSerializer> serializerMock;
            Mock<IRpcChannel> channelMock;
            var proxy = GenerateTestClassProxyClass(expectedResult, expectedValue, expectedInvocationValue,
                out serializerMock,
                out channelMock);

            var returnedValue = proxy.ReturnValue(expectedValue);
            Assert.AreEqual(expectedValue, returnedValue);
        }

        [Test]
        [ExpectedException(typeof (TestException))]
        public void CanInvokeExceptionThrowingMethod()
        {
            Exception expectedRawValue = new TestException();

            var expectedInvocationValue = new SerializedRpcValue
            {
                ArgumentBytes = TestBytes,
                ArgumentType = expectedRawValue.GetType(),
                IsBinaryType = true
            };

            var expectedResult = new RemoteProxyInvocationExceptionResult
            {
                MessageGuid = TestGuid,
                ExceptionValue = expectedInvocationValue
            };
            Mock<IRpcSerializer> serializerMock;
            Mock<IRpcChannel> channelMock;
            var proxy = GenerateTestClassProxyClass(expectedResult, expectedRawValue, expectedInvocationValue,
                out serializerMock,
                out channelMock);
            proxy.ExceptionThrowingMethod();
            serializerMock
                .Verify(serializer => serializer.DeserializeArgumentToObject(expectedResult.ExceptionValue),
                    Times.AtLeastOnce);
        }

        [Test]
        public void CanInvokeParameterlessMethod()
        {
            const string expectedValue = "Test";

            var expectedInvocationValue = new SerializedRpcValue
            {
                ArgumentBytes = TestBytes,
                ArgumentType = expectedValue.GetType(),
                IsBinaryType = true
            };
            var expectedResult = new NonNullRemoteProxyInvocationResult
            {
                MessageGuid = TestGuid,
                ReturnValue = expectedInvocationValue
            };
            Mock<IRpcSerializer> serializerMock;
            Mock<IRpcChannel> channelMock;
            var proxy = GenerateTestClassProxyClass(expectedResult, expectedValue, expectedInvocationValue,
                out serializerMock,
                out channelMock);
            proxy.NonReturningMethod();
        }
    }
}