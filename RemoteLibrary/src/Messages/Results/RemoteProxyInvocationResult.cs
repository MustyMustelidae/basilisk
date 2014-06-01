using System;
using ProtoBuf;

namespace RemoteLibrary.Messages.Results
{
    [ProtoContract]
    [ProtoInclude(100, typeof (NonNullRemoteProxyInvocationResult))]
    [ProtoInclude(200, typeof (NullProxyInvocationResult))]
    [ProtoInclude(300, typeof (RemoteProxyInvocationExceptionResult))]
    [ProtoInclude(400, typeof (VoidProxyInvocationResult))]
    public abstract class RemoteProxyInvocationResult : BaseRpcMessage
    {
        protected RemoteProxyInvocationResult(Guid guid)
        {
            MessageGuid = guid;
        }

        protected RemoteProxyInvocationResult()
        {
        }
    }

    public static class RemoteInvocationResultExt
    {
        public static bool IsNullOrVoid(this RemoteProxyInvocationResult result)
        {
            if(result is NullProxyInvocationResult) return true;

            return result is VoidProxyInvocationResult;
        }
    }
}