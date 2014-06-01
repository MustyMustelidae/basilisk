using RemoteLibrary.Messages;

namespace RemoteLibrary.Util.EventHandlers
{
    public delegate void OnNewInvocationHandler(ref RpcMessage remoteProxyInvocation);
}