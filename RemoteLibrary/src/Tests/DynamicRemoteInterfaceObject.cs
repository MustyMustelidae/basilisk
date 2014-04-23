using System;
using System.Dynamic;
using System.IO;
using System.Reflection;
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
        public interface ITestClass
        {
            object ReturnValue(object val);
            void NonReturningMethod();
            void ExceptionThrowingMethod();
        }
        public class TestException : Exception
        {
             
        }
        public class TestClass : ITestClass
        {
            public object ReturnValue(object val)
            {
                return val;
            }

            public void NonReturningMethod()
            {
                
            }

            public void ExceptionThrowingMethod()
            {
                throw new TestException();
            }
        }

        readonly Type _testType = typeof(ITestClass);
        [Test]
        public void CanInvokeMethodWithParmAndReturnValue()
        {
            const string expectedValue = "Test";
            var testGuid = Guid.Empty;
            var testbytes = testGuid.ToByteArray();

            var serializerMock = new Mock<IRemoteInterfaceSerializer>();

            var expectedResult = new NonNullRemoteInvocationResult
            {
                MessageGuid = testGuid,
                ReturnValue = new SerializedRemoteInvocationValue
                {
                    ArgumentBytes = testbytes,
                    ArgumentType = expectedValue.GetType(),
                    IsBinaryType = true
                },
            };
            serializerMock.Setup(serializer => serializer.SerializeArgumentObject(It.IsAny<object>()))
                .Returns(testbytes);
            serializerMock.Setup(
                serializer => serializer.SerializeMessage(It.IsAny<Stream>(), It.IsAny<RemoteCallMessage>()));
            serializerMock.Setup(
                serializer => serializer.SerializeObjectToInvocationValue(It.IsAny<Type>(), It.IsAny<object>()))
                .Returns(expectedResult.ReturnValue);
            serializerMock.Setup(
                serializer => serializer.DeserializeArgumentToObject(It.IsAny<SerializedRemoteInvocationValue>()))
                .Returns(expectedValue);
            serializerMock.Setup(
                serializer => serializer.DeserializeMessage(It.IsAny<Stream>()))
                .Returns(expectedResult);
            var channelMock = new Mock<IRemoteInterfaceChannel>();
            channelMock.Setup(channel => channel.SendMessageAndWaitForResponse(It.IsAny<RemoteCallMessage>()))
                .Returns(Task.FromResult<RemoteCallMessage>(expectedResult));

            var infoResolverMock = new Mock<ICachedTypeInfoResolver>();
            var guidProviderMock = new Mock<IGuidProvider>();
            var interfaceObject = new DynamicRemoteInterfaceObject(channelMock.Object, infoResolverMock.Object, _testType,
                serializerMock.Object, guidProviderMock.Object);
            var proxy = interfaceObject.ActLike<ITestClass>();

            var returnedValue = (string)proxy.ReturnValue(expectedValue);
            StringAssert.Contains(expectedValue,returnedValue);
        }
        [Test]
        [ExpectedException(typeof(TestException))]
        public void CanInvokeExceptionThrowingMethod()
        {
            Exception expectedRawValue = new TestException();
            var testGuid = Guid.Empty;
            var testbytes = testGuid.ToByteArray();


            var expectedInvocationValue = new SerializedRemoteInvocationValue
            {
                ArgumentBytes = testbytes,
                ArgumentType = expectedRawValue.GetType(),
                IsBinaryType = true
            };

            var expectedResult = new RemoteInvocationExceptionResult()
            {
                MessageGuid = testGuid,
                ExceptionValue = expectedInvocationValue
            };

            var serializerMock = new Mock<IRemoteInterfaceSerializer>();
            serializerMock.Setup(serializer => serializer.SerializeArgumentObject(It.IsAny<object>()))
                .Returns(testbytes);
            serializerMock.Setup(
                serializer => serializer.SerializeMessage(It.IsAny<Stream>(), It.IsAny<RemoteCallMessage>()));
            serializerMock.Setup(
                serializer => serializer.SerializeObjectToInvocationValue(It.IsAny<Type>(), It.IsAny<object>()))
                .Returns(expectedResult.ExceptionValue);
            serializerMock.Setup(
                serializer => serializer.DeserializeArgumentToObject(It.IsAny<SerializedRemoteInvocationValue>()))
                .Returns(expectedRawValue);
            serializerMock.Setup(
                serializer => serializer.DeserializeMessage(It.IsAny<Stream>()))
                .Returns(expectedResult);
            var channelMock = new Mock<IRemoteInterfaceChannel>();
            channelMock.Setup(channel => channel.SendMessageAndWaitForResponse(It.IsAny<RemoteCallMessage>()))
                .Returns(Task.FromResult<RemoteCallMessage>(expectedResult));

            var infoResolverMock = new Mock<ICachedTypeInfoResolver>();
            var guidProviderMock = new Mock<IGuidProvider>();
            var interfaceObject = new DynamicRemoteInterfaceObject(channelMock.Object, infoResolverMock.Object, _testType,
                serializerMock.Object, guidProviderMock.Object);
            var proxy = interfaceObject.ActLike<ITestClass>();
            proxy.ExceptionThrowingMethod();
            serializerMock
                .Verify(serializer => serializer.DeserializeArgumentToObject(expectedResult.ExceptionValue),Times.AtLeastOnce);
        }
        [Test]
        public void CanInvokeMethodWithNoParmsAndNoReturnValue()
        {
            const string expectedValue = "Test";
            var testGuid = Guid.Empty;
            var testbytes = testGuid.ToByteArray();

            var serializerMock = new Mock<IRemoteInterfaceSerializer>();

            var expectedResult = new NonNullRemoteInvocationResult
            {
                MessageGuid = testGuid,
                ReturnValue = new SerializedRemoteInvocationValue
                {
                    ArgumentBytes = testbytes,
                    ArgumentType = expectedValue.GetType(),
                    IsBinaryType = true
                },
            };
            serializerMock.Setup(serializer => serializer.SerializeArgumentObject(It.IsAny<object>()))
                .Returns(testbytes);

            serializerMock.Setup(
                serializer => serializer.SerializeMessage(It.IsAny<Stream>(), It.IsAny<RemoteCallMessage>()));

            serializerMock.Setup(
                serializer => serializer.SerializeObjectToInvocationValue(It.IsAny<Type>(), It.IsAny<object>()))
                .Returns(expectedResult.ReturnValue);

            serializerMock.Setup(
                serializer => serializer.DeserializeArgumentToObject(It.IsAny<SerializedRemoteInvocationValue>()))
                .Returns(expectedValue);

            serializerMock.Setup(
                serializer => serializer.DeserializeMessage(It.IsAny<Stream>()))
                .Returns(expectedResult);

            var channelMock = new Mock<IRemoteInterfaceChannel>();

            channelMock.Setup(channel => channel.SendMessageAndWaitForResponse(It.IsAny<RemoteCallMessage>()))
                .Returns(Task.FromResult<RemoteCallMessage>(expectedResult));

            var infoResolverMock = new Mock<ICachedTypeInfoResolver>();

            var guidProviderMock = new Mock<IGuidProvider>();

            var interfaceObject = new DynamicRemoteInterfaceObject(channelMock.Object, infoResolverMock.Object, _testType,
                serializerMock.Object, guidProviderMock.Object);

            var proxy = interfaceObject.ActLike<ITestClass>();

            proxy.NonReturningMethod();
        }
    }
}