using System;
using System.Collections.Generic;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Util.ArgumentProviders
{
    internal class RemoteInterfaceArgumentProvider : IRemoteInterfaceArgumentProvider
    {
        public RemoteInvocationValue Create(IRemoteInterfaceSerializer serializer, Type type, object sourceObject)
        {
            if (sourceObject == null)
            {
                return NullArgument(type);
            }

            var invocationArgument = new SerializedRemoteInvocationValue
            {
                ArgumentType = type,
                IsBinaryType = serializer.IsBinaryType(type),
                ArgumentBytes = serializer.SerializeArgumentObject(sourceObject)
            };
            return invocationArgument;
        }

        public RemoteInvocationValue Create<T>(IRemoteInterfaceSerializer serializer, T sourceObject)
        {
            var type = typeof (T);
            return Create(serializer, type, sourceObject);
        }

        public RemoteInvocationValue NullArgument(Type type)
        {
            return new NullRemoteInvocationValue(type);
        }

        public IEnumerable<RemoteInvocationValue> CreateRemoteArguments(IRemoteInterfaceSerializer serializer,
            object[] objects)
        {
            var arguments = new RemoteInvocationValue[objects.Length];
            for (var i = 0; i < objects.Length; i++)
            {
                var argObject = objects[i];
                if (argObject is NullRemoteInvocationValue)
                {
                    arguments[i] = argObject as NullRemoteInvocationValue;
                }
                arguments[i] = Create(serializer, argObject);
            }
            return arguments;
        }
    }
}