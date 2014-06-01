using System;
using ProtoBuf;

namespace RemoteLibrary.Messages.Results
{
    [ProtoContract]
    public class NullProxyInvocationResult : RemoteProxyInvocationResult
    {
        public NullProxyInvocationResult()
        {
        }

        public NullProxyInvocationResult(Guid guid, Type returnType) : base(guid)
        {
            ReturnType = returnType;
        }

        [ProtoMember(1)]
        public Type ReturnType { get; set; }
    }
}