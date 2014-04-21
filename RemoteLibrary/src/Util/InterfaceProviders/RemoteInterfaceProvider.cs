using System;
using System.Collections.Generic;
using RemoteLibrary.Exceptions;

namespace RemoteLibrary.Util.InterfaceProviders
{
    internal class RemoteInterfaceProvider : IGenericRemoteInterfaceProvider, IRemoteInterfaceProvider
    {
        private readonly Dictionary<Type, object> _interfaceInstances = new Dictionary<Type, object>();
        private readonly object _interfaceInstancesLock = new object();


        public void RegisterInterface<T>(T instance) where T : class
        {
            var interfaceType = typeof (T);
            RegisterInterface(interfaceType, instance);
        }

        public T GetLocalInstance<T>() where T : class
        {
            var interfaceType = typeof (T);
            return GetLocalInstance(interfaceType) as T;
        }

        public bool IsInterfaceRegistered<T>()
        {
            var interfaceType = typeof (T);
            return IsInterfaceRegistered(interfaceType);
        }

        public void RegisterInterface(Type interfaceType, object instance)
        {
            if (IsInterfaceRegistered(interfaceType)) return;
            lock (_interfaceInstancesLock)
            {
                if (_interfaceInstances.ContainsKey(interfaceType))
                {
                    _interfaceInstances.Add(interfaceType, instance);
                }
            }
        }

        public object GetLocalInstance(Type interfaceType)
        {
            if (!IsInterfaceRegistered(interfaceType)) throw new InstanceNotDefinedException(interfaceType);
            lock (_interfaceInstancesLock)
            {
                return _interfaceInstances[interfaceType];
            }
        }

        public bool IsInterfaceRegistered(Type interfaceType)
        {
            if (!interfaceType.IsInterface) throw new TypeArgumentException("Type must be interface type");
            lock (_interfaceInstancesLock)
            {
                return _interfaceInstances.ContainsKey(interfaceType);
            }
        }
    }
}