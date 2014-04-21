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
    public class RemoteInvocation : RemoteInterfaceMessage, IEquatable<RemoteInvocation>
    {
        public RemoteInvocation(string methodName, Type invocationType,
            IEnumerable<RemoteInvocationValue> methodArguments, IGuidProvider guidProvider)
            : base(guidProvider)
        {
            MethodName = methodName;
            InvocationType = invocationType;
            if (methodArguments == null)
            {
                MethodArguments = new List<RemoteInvocationValue>(0);
                return;
            }

            MethodArguments = new List<RemoteInvocationValue>(methodArguments);
        }


        public RemoteInvocation()
        {
            MethodArguments = new List<RemoteInvocationValue>(0);
        }

        [ProtoMember(1)]
        public string MethodName { get; set; }

        [ProtoMember(2)]
        public Type InvocationType { get; set; }

        [ProtoMember(3)]
        public List<RemoteInvocationValue> MethodArguments { get; set; }

        public bool Equals(RemoteInvocation other)
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

        public object[] ConvertArgsToObjects(IRemoteInterfaceSerializer serializer)
        {

            return MethodArguments.Select(arg => ConvertArgToObject(arg, serializer)).ToArray();
        }

        object ConvertArgToObject(RemoteInvocationValue arg, IRemoteInterfaceSerializer serializer)
        {
            if (arg is SerializedRemoteInvocationValue)
            {
                var serializedArg = arg as SerializedRemoteInvocationValue;
                return serializer.DeserializeArgumentToObject(serializedArg);
            }
            if (arg is NullRemoteInvocationValue)
            {
                return null;
            }
            throw new ArgumentOutOfRangeException("arg");
        }
        public override bool Equals(object obj)
        {
            if (obj is RemoteInvocation)
            {
                return Equals(obj as RemoteInvocation);
            }
            return base.Equals(obj);
        }
    }
}