using System;
using ProtoBuf;

namespace RemoteLibrary.Messages.Results
{
    [ProtoContract]
    [ProtoInclude(100, typeof (NonNullRemoteInvocationResult))]
    [ProtoInclude(200, typeof (NullInvocationResult))]
    [ProtoInclude(300, typeof (RemoteInvocationExceptionResult))]
    [ProtoInclude(400, typeof (VoidInvocationResult))]
    public abstract class RemoteInvocationResult : RemoteCallMessage
    {
        protected RemoteInvocationResult(Guid guid)
        {
            MessageGuid = guid;
        }

        protected RemoteInvocationResult()
        {
        }
    }

    public static class RemoteInvocationResultExt
    {
        public static bool IsNullOrVoid(this RemoteInvocationResult result)
        {
            if(result is NullInvocationResult) return true;

            return result is VoidInvocationResult;
        }
    }
}