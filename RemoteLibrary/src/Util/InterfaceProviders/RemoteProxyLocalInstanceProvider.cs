using System;
using System.Collections.Generic;
using RemoteLibrary.Exceptions;

namespace RemoteLibrary.Util.InterfaceProviders
{
    public class MonitorLockedRemoteProxyLocalInstanceProvider : IGenericRpcCallInstanceResolver, IRpcCallInstanceResolver
    {
        private readonly Dictionary<Type, object> _proxyInstances = new Dictionary<Type, object>();
        private readonly object _proxyInstancesLock = new object();


        public void RegisterLocalProxyInstance<T>(T instance) where T : class
        {
            var interfaceType = typeof (T);
            RegisterLocalProxyInstance(interfaceType, instance);
        }

        public T GetLocalProxyInstance<T>() where T : class
        {
            var interfaceType = typeof (T);
            return GetLocalProxyInstance(interfaceType) as T;
        }

        public bool IsProxyInstanceRegistered<T>()
        {
            var interfaceType = typeof (T);
            return IsProxyInstanceRegistered(interfaceType);
        }

        public void RegisterLocalProxyInstance(Type interfaceType, object instance)
        {
            if (IsProxyInstanceRegistered(interfaceType)) return;
            lock (_proxyInstancesLock)
            {
                if (_proxyInstances.ContainsKey(interfaceType))
                {
                    _proxyInstances.Add(interfaceType, instance);
                }
            }
        }

        public object GetLocalProxyInstance(Type proxyInterfaceType)
        {
            if (!IsProxyInstanceRegistered(proxyInterfaceType)) throw new InstanceNotDefinedException(proxyInterfaceType);
            lock (_proxyInstancesLock)
            {
                return _proxyInstances[proxyInterfaceType];
            }
        }

        public bool IsProxyInstanceRegistered(Type proxyInterfaceType)
        {
            if (!proxyInterfaceType.IsInterface) throw new TypeArgumentException("Type must be interface type");
            lock (_proxyInstancesLock)
            {
                return _proxyInstances.ContainsKey(proxyInterfaceType);
            }
        }
    }

}