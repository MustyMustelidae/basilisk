using System;
using System.Collections.Generic;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;

namespace RemoteLibrary.Util.ArgumentProviders
{
    public interface IRemoteInterfaceArgumentProvider
    {
        RemoteInvocationValue Create(IRemoteInterfaceSerializer serializer, Type type, object sourceObject);
        RemoteInvocationValue Create<T>(IRemoteInterfaceSerializer serializer, T sourceObject);
        RemoteInvocationValue NullArgument(Type type);

        IEnumerable<RemoteInvocationValue> CreateRemoteArguments(IRemoteInterfaceSerializer serializer,
            object[] objects);
    }
}