using System;
using System.Text;
using Moq;
using NUnit.Framework;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;
using Shared;

namespace RemoteLibrary.Tests
{
    [TestFixture]
    public class RemoteInvocationMessageTests
    {
        [Test]
        public void EqualityTest()
        {
            var guidProviderMock = new Mock<IGuidProvider>();
            var serializerMock = new Mock<IRpcSerializer>();

            const string testMethodName = "VoidMethodWithNoParam";

            guidProviderMock.Setup(provider => provider.GetNewGuid())
                .Returns(Guid.Empty);

            serializerMock
                .Setup(serializer => serializer.SerializeArgumentObject(It.IsAny<object>()))
                .Returns(Encoding.Default.GetBytes(testMethodName));

            var testType = typeof (RemoteInvocationMessageTests);

            var messageA = new RpcMessage(testMethodName, testType,
                new[] {new NullRpcValue(testType)}, guidProviderMock.Object);
            var messageB = new RpcMessage(testMethodName, testType,
                new[] {new NullRpcValue(testType)}, guidProviderMock.Object);
            Assert.IsTrue(messageA.Equals(messageB));
            Assert.AreEqual(messageA, messageB);
        }
    }
}