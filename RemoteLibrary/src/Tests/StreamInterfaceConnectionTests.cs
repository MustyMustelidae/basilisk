using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using RemoteLibrary.Connections;
using RemoteLibrary.Messages;
using RemoteLibrary.Serialization;
using RemoteLibrary.Util.StreamIO;

namespace RemoteLibrary.Tests
{
    public class StreamInterfaceConnectionTests : GenericRemoteInterfaceConnectionTests<StreamRpcConnection>
    {
        protected override StreamRpcConnection GetRemoteInterfaceConnection()
        {
            var mockStreamReader = new Mock<IAsyncStreamReader>();
            var mockStreamWriter = new Mock<IAsyncStreamWriter>();
            var mockSerializer = new Mock<IRpcSerializer>();
            return new StreamRpcConnection(mockStreamReader.Object, mockStreamWriter.Object, mockSerializer.Object);
        }

        protected override StreamRpcConnection GetRemoteInterfaceConnectionWithMessageWaiting(
            BaseRpcMessage message)
        {
            var mockStreamReader = new Mock<IAsyncStreamReader>();
            var mockStreamWriter = new Mock<IAsyncStreamWriter>();

            const int operationWaitTime = 10;
                //Simulate the time it takes to send/receive a message, otherwise the thread consumes messages too quickly

            mockStreamReader
                .Setup(reader => reader.ReadBytesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(async (int count, CancellationToken token) =>
                {
                    await Task.Delay(operationWaitTime, token);
                    return new byte[count];
                });

            mockStreamWriter
                .Setup(writer => writer.WriteBytesAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .Returns((byte[] bytes, CancellationToken token) => Task.Delay(operationWaitTime, token));

            var mockSerializer = new Mock<IRpcSerializer>();
            mockSerializer.Setup(serializer => serializer.DeserializeMessage(It.IsAny<Stream>()))
                .Returns(message);

            return new StreamRpcConnection(mockStreamReader.Object, mockStreamWriter.Object, mockSerializer.Object);
        }

        private readonly Dictionary<Guid, BaseRpcMessage> _sentMessages =
            new Dictionary<Guid, BaseRpcMessage>();

        protected override StreamRpcConnection GetRemoteInterfaceConnectionWithMessageSendingChecked(
            out Guid connectionGuid)
        {
            connectionGuid = Guid.NewGuid();

            var mockStreamReader = new Mock<IAsyncStreamReader>();
            var mockStreamWriter = new Mock<IAsyncStreamWriter>();

            const int operationWaitTime = 10;
                //Simulate the time it takes to send/receive a message, otherwise the thread consumes messages too quickly
            mockStreamReader
                .Setup(reader => reader.ReadBytesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(async (int count, CancellationToken token) =>
                {
                    await Task.Delay(operationWaitTime, token);
                    return new byte[count];
                });
            mockStreamWriter
                .Setup(writer => writer.WriteBytesAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .Returns((byte[] bytes, CancellationToken token) => Task.Delay(operationWaitTime, token));
            var mockSerializer = new Mock<IRpcSerializer>();
            var guid = connectionGuid;
            mockSerializer.Setup(
                serializer => serializer.SerializeMessage(It.IsAny<Stream>(), It.IsAny<BaseRpcMessage>()))
                .Callback((Stream stream, BaseRpcMessage message) => _sentMessages.Add(guid, message));

            return new StreamRpcConnection(mockStreamReader.Object, mockStreamWriter.Object, mockSerializer.Object);
        }

        protected override BaseRpcMessage[] CheckConnectionMessages(Guid connectionGuid)
        {
            return _sentMessages
                .ToArray()
                .Where(pair => pair.Key.Equals(connectionGuid))
                .Select(pair => pair.Value)
                .ToArray();
        }
    }
}