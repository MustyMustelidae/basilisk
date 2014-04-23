using System;
using ProtoBuf;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Messages.Results
{
    [ProtoContract]
    public class RemoteInvocationExceptionResult : RemoteInvocationResult
    {
        public RemoteInvocationExceptionResult()
        {
        }

        public RemoteInvocationExceptionResult(Guid guid, Type exceptionType, IRemoteInterfaceSerializer serializer,
            Exception exception)
            : base(guid)
        {
            ExceptionValue = serializer.SerializeObjectToInvocationValue(exceptionType, exception);
        }

        [ProtoMember(1)]
        public SerializedRemoteInvocationValue ExceptionValue { get; set; }

        public static RemoteInvocationExceptionResult FromException<T>(Guid guid, IRemoteInterfaceSerializer serializer,
            T exception) where T : Exception
        {
            return new RemoteInvocationExceptionResult(guid, typeof (T), serializer, exception);
        }
    }
}