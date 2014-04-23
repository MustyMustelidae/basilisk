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
        private interface ITestInterface
        {
            object ReturnValue(object val);
            void NonReturningMethod();
            void ExceptionThrowingMethod();
        }

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

        private ITestInterface GenerateTestClassProxyClass(RemoteCallMessage expectedMessage, object expectedRawValue,
            SerializedRemoteInvocationValue expectedValue, out Mock<IRemoteInterfaceSerializer> serializerMock,
            out Mock<IRemoteInterfaceChannel> channelMock)
        {
            serializerMock = new Mock<IRemoteInterfaceSerializer>();
            serializerMock.Setup(serializer => serializer.SerializeArgumentObject(It.IsAny<object>()))
                .Returns(TestBytes);

            serializerMock.Setup(
                serializer => serializer.SerializeMessage(It.IsAny<Stream>(), It.IsAny<RemoteCallMessage>()));

            serializerMock.Setup(
                serializer => serializer.SerializeObjectToInvocationValue(It.IsAny<Type>(), It.IsAny<object>()))
                .Returns(expectedValue);

            serializerMock.Setup(
                serializer => serializer.DeserializeArgumentToObject(It.IsAny<SerializedRemoteInvocationValue>()))
                .Returns(expectedRawValue);

            serializerMock.Setup(
                serializer => serializer.DeserializeMessage(It.IsAny<Stream>()))
                .Returns(expectedMessage);

            channelMock = new Mock<IRemoteInterfaceChannel>();

            channelMock.Setup(channel => channel.SendMessageAndWaitForResponse(It.IsAny<RemoteCallMessage>()))
                .Returns(Task.FromResult(expectedMessage));

            var infoResolverMock = new Mock<ICachedTypeInfoResolver>();

            var guidProviderMock = new Mock<IGuidProvider>();
            guidProviderMock.Setup(provider => provider.GetNewGuid()).Returns(TestGuid);
            var interfaceObject = new DynamicRemoteInterfaceObject(channelMock.Object, infoResolverMock.Object,
                _testType,
                serializerMock.Object, guidProviderMock.Object);

            var proxy = interfaceObject.ActLike<ITestInterface>();
            return proxy;
        }

        [TestCase("Test")]
        [TestCase(9)]
        public void CanInvokeMethodWithArgsAndParams(object expectedValue)
        {
            var expectedInvocationValue = new SerializedRemoteInvocationValue
            {
                ArgumentBytes = TestBytes,
                ArgumentType = expectedValue.GetType(),
                IsBinaryType = true
            };
            var expectedResult = new NonNullRemoteInvocationResult
            {
                MessageGuid = TestGuid,
                ReturnValue = expectedInvocationValue
            };
            Mock<IRemoteInterfaceSerializer> serializerMock;
            Mock<IRemoteInterfaceChannel> channelMock;
            var proxy = GenerateTestClassProxyClass(expectedResult, expectedValue, expectedInvocationValue,
                out serializerMock,
                out channelMock);

            var returnedValue = proxy.ReturnValue(expectedValue);
            Assert.AreEqual(expectedValue, returnedValue);
        }

        public void CanInvokeMethodWithArgsAndParamsThatAreExceptions()
        {
            var expectedValue = new TestException();
            var expectedInvocationValue = new SerializedRemoteInvocationValue
            {
                ArgumentBytes = TestBytes,
                ArgumentType = expectedValue.GetType(),
                IsBinaryType = true
            };
            var expectedResult = new NonNullRemoteInvocationResult
            {
                MessageGuid = TestGuid,
                ReturnValue = expectedInvocationValue
            };
            Mock<IRemoteInterfaceSerializer> serializerMock;
            Mock<IRemoteInterfaceChannel> channelMock;
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

            var expectedInvocationValue = new SerializedRemoteInvocationValue
            {
                ArgumentBytes = TestBytes,
                ArgumentType = expectedRawValue.GetType(),
                IsBinaryType = true
            };

            var expectedResult = new RemoteInvocationExceptionResult
            {
                MessageGuid = TestGuid,
                ExceptionValue = expectedInvocationValue
            };
            Mock<IRemoteInterfaceSerializer> serializerMock;
            Mock<IRemoteInterfaceChannel> channelMock;
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

            var expectedInvocationValue = new SerializedRemoteInvocationValue
            {
                ArgumentBytes = TestBytes,
                ArgumentType = expectedValue.GetType(),
                IsBinaryType = true
            };
            var expectedResult = new NonNullRemoteInvocationResult
            {
                MessageGuid = TestGuid,
                ReturnValue = expectedInvocationValue
            };
            Mock<IRemoteInterfaceSerializer> serializerMock;
            Mock<IRemoteInterfaceChannel> channelMock;
            var proxy = GenerateTestClassProxyClass(expectedResult, expectedValue, expectedInvocationValue,
                out serializerMock,
                out channelMock);
            proxy.NonReturningMethod();
        }
    }
}