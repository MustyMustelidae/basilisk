using System;

namespace RemoteLibrary.InterfaceProviders
{
    internal interface IRemoteInterfaceProvider
    {
        T GetProxy<T>();
    }

    internal class RemoteInterfaceProvider : IRemoteInterfaceProvider
    {
        public T GetProxy<T>()
        {
            throw new NotImplementedException();
        }
    }
}