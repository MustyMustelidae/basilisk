using System;
using ProtoBuf;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Messages.Values
{
    [ProtoContract]
    internal class NullRemoteInvocationValue : RemoteInvocationValue
    {
        public NullRemoteInvocationValue(Type objectType) : base(objectType)
        {
        }
    }
}