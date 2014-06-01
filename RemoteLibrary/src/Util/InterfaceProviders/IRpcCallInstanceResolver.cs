using System;

namespace RemoteLibrary.Util.InterfaceProviders
{
    public interface IRpcCallInstanceResolver
    {
        void RegisterLocalProxyInstance(Type interfaceType, object instance);
        object GetLocalProxyInstance(Type interfaceType);
        bool IsProxyInstanceRegistered(Type interfaceType);
    }
}