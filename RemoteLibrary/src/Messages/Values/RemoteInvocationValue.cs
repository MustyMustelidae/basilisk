using System;
using ProtoBuf;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Messages.Values
{
    [ProtoContract]
    [ProtoInclude(100, typeof (NullRemoteInvocationValue))]
    [ProtoInclude(200, typeof (SerializedRemoteInvocationValue))]
    public abstract class RemoteInvocationValue
    {
        protected internal RemoteInvocationValue()
        {
        }

        protected RemoteInvocationValue(Type objectType)
        {
            ArgumentType = objectType;
        }


        [ProtoMember(1)]
        public Type ArgumentType { get; set; }




        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            var otherVal = (RemoteInvocationValue) obj;
            return ArgumentType.Equals(otherVal.ArgumentType);
        }

        // ReSharper disable NonReadonlyFieldInGetHashCode
        public override int GetHashCode()
        {
            return ArgumentType.GetHashCode();
        }
    }
}