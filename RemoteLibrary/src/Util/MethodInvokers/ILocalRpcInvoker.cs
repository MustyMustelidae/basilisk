using System.Threading;
using System.Threading.Tasks;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Results;

namespace RemoteLibrary.Util.MethodInvokers
{
    public interface ILocalRpcInvoker
    {
        Task<RemoteProxyInvocationResult> InvokeMessageLocal(RpcMessage proxyInvocationMessage,
            CancellationToken cancelToken);
    }
}