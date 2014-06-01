using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ImpromptuInterface;
using RemoteLibrary.Exceptions;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Results;
using RemoteLibrary.Serialization;
using RemoteLibrary.Util.Cached;
using RemoteLibrary.Util.InterfaceProviders;
using Shared;

namespace RemoteLibrary.Util.MethodInvokers
{
    public class TplLocalRpcInvoker : ILocalRpcInvoker
    {
        private readonly ICachedMethodInvoker _cachedMethodInvoker;
        private readonly ICachedTypeResolver _cachedTypeResolver;
        private readonly IRpcCallInstanceResolver _proxyLocalInstanceProvider;
        private readonly IRpcSerializer _proxySerializer;

        public TplLocalRpcInvoker(IRpcCallInstanceResolver proxyLocalInstanceProvider,
            IRpcSerializer proxySerializer, ICachedTypeResolver cachedTypeResolver,
            ICachedMethodInvoker cachedMethodInvoker)
        {
            _proxyLocalInstanceProvider = proxyLocalInstanceProvider;
            _proxySerializer = proxySerializer;
            _cachedTypeResolver = cachedTypeResolver;
            _cachedMethodInvoker = cachedMethodInvoker;
        }

        public async Task<RemoteProxyInvocationResult> InvokeMessageLocal(RpcMessage proxyInvocationMessage,
            CancellationToken cancelToken)
        {
            Debug.Assert(proxyInvocationMessage != null);
            var invocationGuid = proxyInvocationMessage.MessageGuid;
            try
            {
                var targetType = proxyInvocationMessage.InvocationType;
                var methodName = proxyInvocationMessage.MethodName;
                var methodArgs = proxyInvocationMessage.ConvertArgsToObjects(_proxySerializer);

                var methodInfo = _cachedTypeResolver.GetMethodByName(targetType, methodName);
                if (methodInfo == null)
                {
                    var exception = new RemoteInvocationException(
                        "Target type does not contain method with name: {0}".SFormat(methodName));

                    return RemoteProxyInvocationExceptionResult.FromException(invocationGuid, _proxySerializer, exception);
                }
                var returnType = methodInfo.ReturnType;

                var invocationResult = await InvokeLocal(cancelToken, targetType, methodInfo, methodArgs);

                if (proxyInvocationMessage.InvocationType == typeof (void))
                {
                    return new VoidProxyInvocationResult(invocationGuid);
                }
                return new NonNullRemoteProxyInvocationResult(invocationGuid, returnType, _proxySerializer,
                    invocationResult);
            }
            catch (Exception exception)
            {
                return new RemoteProxyInvocationExceptionResult(invocationGuid, exception.GetType(), _proxySerializer,
                    exception);
            }
        }


        private async Task<object> InvokeLocal(CancellationToken cancelToken, Type targetType, MethodInfo methodInfo,
            object[] methodArgs)
        {
            if (!_proxyLocalInstanceProvider.IsProxyInstanceRegistered(targetType))
            {
                throw new RemoteInvocationException("Instance not defined for invocation's target type");
            }
            var targetInstance = _proxyLocalInstanceProvider.GetLocalProxyInstance(targetType);

            var returnType = methodInfo.ReturnType;

            try
            {
                var isTask = typeof (Task).IsGenericAssignableFrom(returnType);
                if (isTask)
                {
                    return await InvokeLocalTask(methodArgs, returnType, targetInstance, methodInfo);
                }
                return await InvokeLocalMethod(cancelToken, targetType, methodInfo, methodArgs, targetInstance);
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException != null)
                {
                    throw exception.InnerException;
                }
                throw;
            }
        }

        private async Task<object> InvokeLocalMethod(CancellationToken cancelToken, Type targetType,
            MethodInfo methodInfo,
            object[] methodArgs, object targetInstance)
        {
            var methodFunc =
                new Func<object>(() => _cachedMethodInvoker.Invoke(targetType, targetInstance, methodInfo, methodArgs));
            return await Task<object>.Factory.StartNew(methodFunc, cancelToken);
        }

        private async Task<object> InvokeLocalTask(object[] methodArgs, Type returnType, object targetInstance,
            MethodInfo methodInfo)
        {
            var isGenericTask = typeof (Task<>).IsGenericAssignableFrom(returnType);
            if (isGenericTask)
            {
                return (await _cachedMethodInvoker.Invoke(returnType, targetInstance, methodInfo, methodArgs)
                    .ActLike(typeof (Task<object>)));
            }
            await (Task) _cachedMethodInvoker.Invoke(returnType, targetInstance, methodInfo, methodArgs);
            return null;
        }
    }
}