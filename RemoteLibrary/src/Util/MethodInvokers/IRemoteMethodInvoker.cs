using System.Threading;
using System.Threading.Tasks;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Results;

namespace RemoteLibrary.Util.MethodInvokers
{
    public interface IRemoteMethodInvoker
    {
        Task<RemoteInvocationResult> InvokeMessageLocal(RemoteInvocation invocationMessage,
            CancellationToken cancelToken);
    }
}