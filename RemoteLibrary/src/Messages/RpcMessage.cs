using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;
using Shared;

namespace RemoteLibrary.Messages
{
    [ProtoContract]
    public class RpcMessage : BaseRpcMessage, IEquatable<RpcMessage>
    {
        public RpcMessage(string methodName, Type invocationType,
            IEnumerable<RpcValue> methodArguments, IGuidProvider guidProvider)
            : base(guidProvider)
        {
            MethodName = methodName;
            InvocationType = invocationType;
            if (methodArguments == null)
            {
                MethodArguments = new List<RpcValue>(0);
                return;
            }

            MethodArguments = new List<RpcValue>(methodArguments);
        }


        public RpcMessage()
        {
            MethodArguments = new List<RpcValue>(0);
        }

        [ProtoMember(1)]
        public string MethodName { get; set; }

        [ProtoMember(2)]
        public Type InvocationType { get; set; }

        [ProtoMember(3)]
        public List<RpcValue> MethodArguments { get; set; }

        public bool Equals(RpcMessage other)
        {
            if (other == null) return false;
            var argsEqual = MethodArguments.SequenceEqual(other.MethodArguments);
            var methodEqual = string.Equals(MethodName, other.MethodName);

            var guidEqual = Equals(MessageGuid, other.MessageGuid);
            return argsEqual && methodEqual && guidEqual;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (MethodName != null ? MethodName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (InvocationType != null ? InvocationType.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (MethodArguments != null ? MethodArguments.GetHashCode() : 0);
                return hashCode;
            }
        }

        public object[] ConvertArgsToObjects(IRpcSerializer serializer)
        {

            return MethodArguments.Select(arg => ConvertArgToObject(arg, serializer)).ToArray();
        }

        object ConvertArgToObject(RpcValue arg, IRpcSerializer serializer)
        {
            if (arg is SerializedRpcValue)
            {
                var serializedArg = arg as SerializedRpcValue;
                return serializer.DeserializeArgumentToObject(serializedArg);
            }
            if (arg is NullRpcValue)
            {
                return null;
            }
            throw new ArgumentOutOfRangeException("arg");
        }
        public override bool Equals(object obj)
        {
            if (obj is RpcMessage)
            {
                return Equals(obj as RpcMessage);
            }
            return base.Equals(obj);
        }
    }
}