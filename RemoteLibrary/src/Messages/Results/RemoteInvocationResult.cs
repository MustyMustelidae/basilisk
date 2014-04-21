using System;
using ProtoBuf;

namespace RemoteLibrary.Messages.Results
{
    [ProtoContract]
    [ProtoInclude(100, typeof (NonNullRemoteInvocationResult))]
    [ProtoInclude(200, typeof (NullInvocationResult))]
    [ProtoInclude(300, typeof (RemoteInvocationExceptionResult))]
    [ProtoInclude(400, typeof (VoidInvocationResult))]
    public abstract class RemoteInvocationResult : RemoteInterfaceMessage
    {
        protected RemoteInvocationResult(Guid guid)
        {
            MessageGuid = guid;
        }

        protected RemoteInvocationResult()
        {
        }
    }
}