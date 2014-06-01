using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using RemoteLibrary.Messages;
using RemoteLibrary.Serialization;
using RemoteLibrary.Util.StreamIO;

namespace RemoteLibrary.Connections
{
    public class StreamRpcConnection : IRpcConnection
    {
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly Task _receiveLoopTask;
        private readonly Task _sendLoopTask;
        private readonly IRpcSerializer _serializer;
        private readonly IAsyncStreamReader _streamReader;
        private readonly IAsyncStreamWriter _streamWriter;
        protected BufferBlock<BaseRpcMessage> ReceivedMessageBlock;
        protected BufferBlock<BaseRpcMessage> UnsentMessageBlock;

        public StreamRpcConnection(IAsyncStreamReader streamReader, IAsyncStreamWriter streamWriter,
            IRpcSerializer serializer)
        {
            if (streamReader == null) throw new ArgumentNullException("streamReader");
            if (streamWriter == null) throw new ArgumentNullException("streamWriter");
            if (serializer == null) throw new ArgumentNullException("serializer");

            _streamReader = streamReader;
            _streamWriter = streamWriter;
            _serializer = serializer;

            ReceivedMessageBlock = new BufferBlock<BaseRpcMessage>();
            UnsentMessageBlock = new BufferBlock<BaseRpcMessage>();
            _cancelTokenSource = new CancellationTokenSource();

            _receiveLoopTask = ReceiveLoop(_cancelTokenSource)
                .ContinueWith(ReceiveLoopExceptionHandler, TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(task => Dispose());

            _sendLoopTask = SendLoop(_cancelTokenSource)
                .ContinueWith(SendLoopExceptionHandler, TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(task => Dispose());
            IsConnected = true;
        }

        public bool IsConnected { get; private set; }

        public async Task<BaseRpcMessage> GetMessage(CancellationToken cToken)
        {
            if (cToken == default(CancellationToken)) throw new ArgumentNullException("cToken");
            return await ReceivedMessageBlock.ReceiveAsync(cToken);
        }

        public void SendMessage(BaseRpcMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");
            UnsentMessageBlock.Post(message);
        }

        protected virtual void Dispose(bool closeStreams)
        {
            Close(closeStreams);

            _cancelTokenSource.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
        }


        private void SendLoopExceptionHandler(Task sendLoopTask)
        {
            if (sendLoopTask == null) throw new ArgumentNullException("sendLoopTask");
            Debugger.Break();
        }

        private void ReceiveLoopExceptionHandler(Task receiveLoopTask)
        {
            if (receiveLoopTask == null) throw new ArgumentNullException("receiveLoopTask");
            Debugger.Break();
        }

        private async Task TryEndLoops()
        {
            try
            {
                _cancelTokenSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                await Task.WhenAny(_receiveLoopTask);
            }
            catch (Exception)
            {
                Debugger.Break();
            }

            try
            {
                await Task.WhenAny(_sendLoopTask);
            }  

            catch (Exception)
            {
                Debugger.Break();
            }
        }

        private async Task ReceiveLoop(CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource == null) throw new ArgumentNullException("cancellationTokenSource");

            var cancelToken = cancellationTokenSource.Token;
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                await HandleNewMessage(cancelToken);
            }
        }

        private async Task HandleNewMessage(CancellationToken cancelToken)
        {
            if (cancelToken == null) throw new ArgumentNullException("cancelToken");
            const int headerSize = sizeof (int);

            var header = await _streamReader.ReadBytesAsync(headerSize, cancelToken);

            var messageSize = BitConverter.ToInt32(header, 0);
            
            var message = await _streamReader.ReadBytesAsync(messageSize, cancelToken);
            Array.Copy(message, 0, header, header.Length - 1, message.Length);
            using (var ms = new MemoryStream(header))
            {
                var newMessage = _serializer.DeserializeMessage(ms);
                ReceivedMessageBlock.Post(newMessage);
            }
        }

        private async Task SendLoop(CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource == null) throw new ArgumentNullException("cancellationTokenSource");

            var cancelToken = cancellationTokenSource.Token;
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var unsentMessage = await UnsentMessageBlock.ReceiveAsync(cancelToken);
                byte[] messageBytes;

                using (var ms = new MemoryStream())
                {
                    _serializer.SerializeMessage(ms, unsentMessage);
                    messageBytes = ms.ToArray();
                }

                await _streamWriter.WriteBytesAsync(messageBytes, cancelToken);
            }
        }

        public void Close(bool waitForClose,int timeout = 10)
        {
            IsConnected = false;
            if (waitForClose) TryEndLoops().Wait(timeout);
            Task.Run(() => TryEndLoops())
                .ConfigureAwait(false);
        }
    }

    class NetworkStreamRpcConnection : StreamRpcConnection
    {
        private  static AsyncStreamReader AsyncReaderFromStream(NetworkStream networkStream)
        {
            if (networkStream == null) throw new ArgumentNullException("networkStream");
            return new AsyncStreamReader(networkStream);
        }

        private static AsyncStreamWriter AsyncWriterFromStream(NetworkStream networkStream)
        {
            if (networkStream == null) throw new ArgumentNullException("networkStream");
            return new AsyncStreamWriter(networkStream);
        }
        public NetworkStreamRpcConnection(NetworkStream networkStream,
           IRpcSerializer serializer)
            : base(AsyncReaderFromStream(networkStream), AsyncWriterFromStream(networkStream), serializer)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
        }
        public NetworkStreamRpcConnection(IAsyncStreamReader streamReader, IAsyncStreamWriter streamWriter, IRpcSerializer serializer) : base(streamReader, streamWriter, serializer)
        {
        }
    }
}