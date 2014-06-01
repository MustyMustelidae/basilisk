using System;
using ProtoBuf;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Messages.Results
{
    [ProtoContract]
    public class RemoteProxyInvocationExceptionResult : RemoteProxyInvocationResult
    {
        public RemoteProxyInvocationExceptionResult()
        {
        }

        public RemoteProxyInvocationExceptionResult(Guid guid, Type exceptionType, IRpcSerializer serializer,
            Exception exception)
            : base(guid)
        {
            ExceptionValue = serializer.SerializeObjectToInvocationValue(exceptionType, exception);
        }

        [ProtoMember(1)]
        public SerializedRpcValue ExceptionValue { get; set; }

        public static RemoteProxyInvocationExceptionResult FromException<T>(Guid guid, IRpcSerializer serializer,
            T exception) where T : Exception
        {
            return new RemoteProxyInvocationExceptionResult(guid, typeof (T), serializer, exception);
        }
    }
}