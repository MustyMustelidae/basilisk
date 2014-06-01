using System;
using ProtoBuf;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Messages.Results
{
    [ProtoInclude(100, typeof (NonNullRemoteProxyInvocationResult))]
    public class NonNullRemoteProxyInvocationResult : RemoteProxyInvocationResult
    {
        public NonNullRemoteProxyInvocationResult()
        {
        }
        
        public NonNullRemoteProxyInvocationResult(Guid guid, Type resultType, IRpcSerializer serializer,
            object resultObject) : base(guid)
        {
            ReturnValue = serializer.SerializeObjectToInvocationValue(resultType, resultObject);
        }

        [ProtoMember(1)]
        public SerializedRpcValue ReturnValue { get;  set; }

        public Type ReturnType{get { return ReturnValue.ArgumentType; }}
    }
}