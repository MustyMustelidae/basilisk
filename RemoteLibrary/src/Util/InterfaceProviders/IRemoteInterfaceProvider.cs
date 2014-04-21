using System;

namespace RemoteLibrary.Util.InterfaceProviders
{
    public interface IRemoteInterfaceProvider
    {
        void RegisterInterface(Type interfaceType, object instance);
        object GetLocalInstance(Type interfaceType);
        bool IsInterfaceRegistered(Type interfaceType);
    }
}