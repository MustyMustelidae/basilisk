﻿using System;
using System.IO;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Values;

namespace RemoteLibrary.Serialization
{
    public interface IRemoteInterfaceSerializer
    {
        RemoteInterfaceMessage DeserializeMessage(Stream stream);
        void SerializeMessage(Stream stream, RemoteInterfaceMessage interfaceMessage);
        byte[] SerializeArgumentObject(object argumentObject);
        SerializedRemoteInvocationValue SerializeObjectToInvocationValue(Type objectType, object valueObject);
        object DeserializeArgumentToObject(SerializedRemoteInvocationValue argument);
        bool IsBinaryType(Type type);
    }
}