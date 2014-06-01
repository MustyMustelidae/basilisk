using ImpromptuInterface;

namespace RemoteLibrary.InterfaceProviders
{
    public class DynamicRpcProvider : IRpcRemoteProvider
    {
        private readonly IRpcDynamicObject _remoteProxyObject;
        public DynamicRpcProvider(IRpcDynamicObject remoteProxyObject)
        {
            _remoteProxyObject = remoteProxyObject;
        }
        public T GetProxy<T>() where T : class
        {
            return _remoteProxyObject.ActLike<T>(); 
        }
    }
}