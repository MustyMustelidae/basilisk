using System;
using ProtoBuf;

namespace RemoteLibrary.Messages.Results
{
    [ProtoInclude(400, typeof (VoidProxyInvocationResult))]
    public class VoidProxyInvocationResult : RemoteProxyInvocationResult
    {
        public VoidProxyInvocationResult()
        {
        }

        public VoidProxyInvocationResult(Guid guid) : base(guid)
        {
        }
    }
}