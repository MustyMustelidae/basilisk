using System;
using ProtoBuf;

namespace RemoteLibrary.Messages.Results
{
    [ProtoContract]
    public class NullInvocationResult : RemoteInvocationResult
    {
        public NullInvocationResult()
        {
        }

        public NullInvocationResult(Guid guid, Type returnType) : base(guid)
        {
            ReturnType = returnType;
        }

        [ProtoMember(1)]
        public Type ReturnType { get; set; }
    }
}