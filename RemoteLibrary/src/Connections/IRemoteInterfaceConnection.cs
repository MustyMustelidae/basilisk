using System;
using System.Threading;
using System.Threading.Tasks;
using RemoteLibrary.Messages;

namespace RemoteLibrary.Connections
{
    public interface IRemoteInterfaceConnection : IDisposable
    {
        Task<RemoteCallMessage> GetMessage(CancellationToken cToken);
        void SendMessage(RemoteCallMessage message);
    }
}