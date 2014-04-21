using System;
using System.Threading;
using NUnit.Framework;
using ProtoBuf;
using RemoteLibrary.Connections;
using RemoteLibrary.Messages;
using RemoteLibrary.Properties;

namespace RemoteLibrary.Tests
{
    [TestFixture]
    public abstract class GenericRemoteInterfaceConnectionTests<T> where T : IRemoteInterfaceConnection
    {
        [ProtoContract]
        protected class TestRemoteInterfaceMessage : RemoteInterfaceMessage
        {
            [UsedImplicitly]
            public TestRemoteInterfaceMessage()
            {
                MessageGuid = Guid.NewGuid();
            }

            public TestRemoteInterfaceMessage(Guid guid)
            {
                MessageGuid = guid;
            }
        }

        protected abstract T GetRemoteInterfaceConnection();
        protected abstract T GetRemoteInterfaceConnectionWithMessageWaiting(RemoteInterfaceMessage message);

        protected abstract T GetRemoteInterfaceConnectionWithMessageSendingChecked(out Guid connectionGuid);
        protected abstract RemoteInterfaceMessage[] CheckConnectionMessages(Guid connectionGuid);

        [Test]
        public async void CanGetMessage()
        {
            var testMessage = new TestRemoteInterfaceMessage();
            using (var cancelSource = new CancellationTokenSource())
            using (var connection = GetRemoteInterfaceConnectionWithMessageWaiting(testMessage))
            {
                var receivedMessage = await connection.GetMessage(cancelSource.Token);
                Assert.NotNull(receivedMessage);
                Assert.AreEqual(testMessage.MessageGuid, receivedMessage.MessageGuid);
            }
        }

        [Test]
        public void CanSendMessage()
        {
            var testMessage = new TestRemoteInterfaceMessage();
            Guid connectionGuid;
            using (var connection = GetRemoteInterfaceConnectionWithMessageSendingChecked(out connectionGuid))
            {
                connection.SendMessage(testMessage);


                RemoteInterfaceMessage[] messages;
                do
                {
                    messages = CheckConnectionMessages(connectionGuid);
                } while (messages.Length < 1);

                CollectionAssert.IsNotEmpty(messages);
                CollectionAssert.Contains(messages, testMessage);
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async void GetMessageThrowsExceptionOnNullCancelToken()
        {
            using (var connection = GetRemoteInterfaceConnection())
            {
                await connection.GetMessage(default(CancellationToken));
            }
        }
    }
}