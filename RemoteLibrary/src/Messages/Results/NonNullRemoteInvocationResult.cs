using System;
using ProtoBuf;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Messages.Results
{
    [ProtoInclude(100, typeof (NonNullRemoteInvocationResult))]
    public class NonNullRemoteInvocationResult : RemoteInvocationResult
    {
        public NonNullRemoteInvocationResult()
        {
        }
        
        public NonNullRemoteInvocationResult(Guid guid, Type resultType, IRemoteInterfaceSerializer serializer,
            object resultObject) : base(guid)
        {
            ReturnValue = serializer.SerializeObjectToInvocationValue(resultType, resultObject);
        }

        [ProtoMember(1)]
        public SerializedRemoteInvocationValue ReturnValue { get;  set; }

        public Type ReturnType{get { return ReturnValue.ArgumentType; }}
    }
}