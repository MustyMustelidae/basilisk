using System;
using ProtoBuf;

namespace RemoteLibrary.Messages.Values
{
    [ProtoContract]
    [ProtoInclude(100, typeof (NullRpcValue))]
    [ProtoInclude(200, typeof (SerializedRpcValue))]
    public abstract class RpcValue
    {
        protected internal RpcValue()
        {
        }

        protected RpcValue(Type objectType)
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
            var otherVal = (RpcValue) obj;
            return ArgumentType.Equals(otherVal.ArgumentType);
        }

        // ReSharper disable NonReadonlyFieldInGetHashCode
        public override int GetHashCode()
        {
            return ArgumentType.GetHashCode();
        }
    }
}