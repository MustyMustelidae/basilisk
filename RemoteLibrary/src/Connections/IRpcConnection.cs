using System;
using System.Threading;
using System.Threading.Tasks;
using RemoteLibrary.Messages;

namespace RemoteLibrary.Connections
{
    /// <summary>
    /// Interface IRpcConnection
    /// </summary>
    public interface IRpcConnection : IDisposable
    {
        /// <summary>
        /// Gets a new message.
        /// </summary>
        /// <param name="cToken">The c token.</param>
        /// <returns>Task&lt;BaseRemoteProxyInvocationMessage&gt;.</returns>
        Task<BaseRpcMessage> GetMessage(CancellationToken cToken);
        /// <summary>
        /// Sends a new message.
        /// </summary>
        /// <param name="message">The message.</param>
        void SendMessage(BaseRpcMessage message);
    }
}