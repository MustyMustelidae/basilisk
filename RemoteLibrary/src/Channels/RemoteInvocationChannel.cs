using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RemoteLibrary.Connections;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Results;
using RemoteLibrary.Properties;
using RemoteLibrary.Serialization;
using RemoteLibrary.Util;
using RemoteLibrary.Util.EventHandlers;
using RemoteLibrary.Util.MethodInvokers;

namespace RemoteLibrary.Channels
{
    internal class RemoteInvocationChannel : IRemoteInterfaceChannel, IDisposable
    {
        private const int MaxSentMessages = Int32.MaxValue;
        private static readonly SemaphoreSlim SendSemaphore = new SemaphoreSlim(MaxSentMessages);
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly Task _commLoopTask;
        private readonly IRemoteInterfaceConnection _interfaceConnection;
        private readonly IRemoteMethodInvoker _methodInvoker;
        protected ConcurrentDictionary<Guid, TaskCompletionSource<RemoteInvocationResult>> AwaitedInvocations;

        public OnNewInvocationHandler OnNewInvocation = delegate { };

        public RemoteInvocationChannel([NotNull] IRemoteInterfaceConnection connection,
            [NotNull] IRemoteMethodInvoker methodInvoker)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (methodInvoker == null) throw new ArgumentNullException("methodInvoker");

            _interfaceConnection = connection;
            _methodInvoker = methodInvoker;

            _cancelTokenSource = new CancellationTokenSource();

            AwaitedInvocations = new ConcurrentDictionary<Guid, TaskCompletionSource<RemoteInvocationResult>>();

            _commLoopTask = CommunicationLoop(_cancelTokenSource)
                .ContinueWith(CommunicationLoopExceptionHandler, TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(task => Close());

            IsConnected = true;
        }

        public void Dispose()
        {
            Close();
        }

        public bool IsConnected { get; private set; }

        public async Task<RemoteCallMessage> SendMessageAndWaitForResponse([NotNull] RemoteCallMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");
            var guid = message.MessageGuid;
            await SendSemaphore.WaitAsync();
            var taskSource = new TaskCompletionSource<RemoteInvocationResult>();

            var taskRegistered = AwaitedInvocations.TryAdd(guid, taskSource);


            if (!taskRegistered)
            {
                throw new ArgumentException("Message with given guid already sent and not received.");
            }

            _interfaceConnection.SendMessage(message);

            var result = await taskSource.Task;

            AwaitedInvocations.TryRemove(guid, out taskSource);

            SendSemaphore.Release();
            return result;
        }

        public static RemoteInvocationChannel FromNetworkStream([NotNull] NetworkStream networkStream,
            [NotNull] IRemoteMethodInvoker methodInvoker, [NotNull] IRemoteInterfaceSerializer serializer)
        {
            if (networkStream == null) throw new ArgumentNullException("networkStream");
            if (methodInvoker == null) throw new ArgumentNullException("methodInvoker");
            if (serializer == null) throw new ArgumentNullException("serializer");
            var interfaceConnection = StreamInterfaceConnection.FromNetworkStream(networkStream, serializer);

            return new RemoteInvocationChannel(interfaceConnection, methodInvoker);
        }


        protected void CommunicationLoopExceptionHandler(Task receiveLoopTask)
        {
        }

        private void TryEndCommunicationLoop()
        {
            try
            {
                _cancelTokenSource.Cancel();
            }
            catch (Exception)
            {
                Debugger.Break();
                return;
            }

            try
            {
                Task.WhenAny(_commLoopTask)
                    .Wait();
            }
            catch (Exception)
            {
                Debugger.Break();
            }
        }

        private async Task CommunicationLoop(CancellationTokenSource cancellationTokenSource)
        {
            var cancelToken = cancellationTokenSource.Token;
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var newMessage = await _interfaceConnection.GetMessage(cancelToken);
                await HandleNewMessage(newMessage, cancelToken);
            }
        }

        private async Task HandleNewMessage(RemoteCallMessage message, CancellationToken cancelToken)
        {
            if (message is RemoteInvocation)
            {
                var invocationMessage = message as RemoteInvocation;
                if (OnNewInvocation != null)
                {
                    OnNewInvocation(ref invocationMessage);
                }
                var invocationResult = await _methodInvoker.InvokeMessageLocal(invocationMessage, cancelToken);
                _interfaceConnection.SendMessage(invocationResult);
                return;
            }
            if (message is RemoteInvocationResult)
            {
                var invocationResult = message as RemoteInvocationResult;
                var messageGuid = invocationResult.MessageGuid;

                Debug.Assert(AwaitedInvocations.ContainsKey(messageGuid),
                    "AwaitedInvocations.ContainsKey(messageGuid)");

                AwaitedInvocations[messageGuid]
                    .SetResult(invocationResult);
            }
        }

        public void Close()
        {
            IsConnected = false;
            TryEndCommunicationLoop();
            _cancelTokenSource.Dispose();
        }
    }
}