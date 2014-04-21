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
    internal class RemoteMethodInvoker : IRemoteMethodInvoker
    {
        private readonly ICachedMethodInvoker _cachedMethodInvoker;
        private readonly ICachedTypeInfoResolver _cachedTypeInfoResolver;
        private readonly IRemoteInterfaceProvider _interfaceProvider;
        private readonly IRemoteInterfaceSerializer _interfaceSerializer;

        public RemoteMethodInvoker(IRemoteInterfaceProvider interfaceProvider,
            IRemoteInterfaceSerializer interfaceSerializer, ICachedTypeInfoResolver cachedTypeInfoResolver,
            ICachedMethodInvoker cachedMethodInvoker)
        {
            _interfaceProvider = interfaceProvider;
            _interfaceSerializer = interfaceSerializer;
            _cachedTypeInfoResolver = cachedTypeInfoResolver;
            _cachedMethodInvoker = cachedMethodInvoker;
        }

        public async Task<RemoteInvocationResult> InvokeMessageLocal(RemoteInvocation invocationMessage,
            CancellationToken cancelToken)
        {
            Debug.Assert(invocationMessage != null);
            var invocationGuid = invocationMessage.MessageGuid;
            try
            {
                var targetType = invocationMessage.InvocationType;
                var methodName = invocationMessage.MethodName;
                var methodArgs = invocationMessage.ConvertArgsToObjects(_interfaceSerializer);

                var methodInfo = _cachedTypeInfoResolver.GetMethodByName(targetType, methodName);
                if (methodInfo == null)
                {
                    var exception = new RemoteInvocationException(
                        "Target type does not contain method with name: {0}".SFormat(methodName));

                    return RemoteInvocationExceptionResult.FromException(invocationGuid, _interfaceSerializer, exception);
                }
                var returnType = methodInfo.ReturnType;

                var invocationResult = await InvokeLocal(cancelToken, targetType, methodInfo, methodArgs);

                if (invocationMessage.InvocationType == typeof (void))
                {
                    return new VoidInvocationResult(invocationGuid);
                }
                return new NonNullRemoteInvocationResult(invocationGuid, returnType, _interfaceSerializer,
                    invocationResult);
            }
            catch (Exception exception)
            {
                return new RemoteInvocationExceptionResult(invocationGuid, exception.GetType(), _interfaceSerializer,
                    exception);
            }
        }


        private async Task<object> InvokeLocal(CancellationToken cancelToken, Type targetType, MethodInfo methodInfo,
            object[] methodArgs)
        {
            if (!_interfaceProvider.IsInterfaceRegistered(targetType))
            {
                throw new RemoteInvocationException("Instance not defined for invocation's target type");
            }
            var targetInstance = _interfaceProvider.GetLocalInstance(targetType);

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