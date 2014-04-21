using System;
using System.Linq;
using ProtoBuf;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Messages.Values
{
    public class SerializedRemoteInvocationValue : RemoteInvocationValue
    {
        protected internal SerializedRemoteInvocationValue()
        {
        }

        [ProtoMember(1)]
        public byte[] ArgumentBytes { get; set; }


        [ProtoMember(2)]
        public bool IsBinaryType { get; set; }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            var otherVal = (SerializedRemoteInvocationValue) obj;
            return IsBinaryType.Equals(otherVal.IsBinaryType)
                   && ArgumentBytes.SequenceEqual(otherVal.ArgumentBytes)
                   && ArgumentType == otherVal.ArgumentType;
        }

        // ReSharper disable NonReadonlyFieldInGetHashCode
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ArgumentBytes != null ? ArgumentBytes.GetHashCode() : 0);

                hashCode = (hashCode*397) ^ (ArgumentType != null ? ArgumentType.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ IsBinaryType.GetHashCode();
                return hashCode;
            }
        }
    }
}