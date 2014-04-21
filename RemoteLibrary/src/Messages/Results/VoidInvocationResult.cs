using System;
using ProtoBuf;

namespace RemoteLibrary.Messages.Results
{
    [ProtoInclude(400, typeof (VoidInvocationResult))]
    public class VoidInvocationResult : RemoteInvocationResult
    {
        public VoidInvocationResult()
        {
        }

        public VoidInvocationResult(Guid guid) : base(guid)
        {
        }
    }
}