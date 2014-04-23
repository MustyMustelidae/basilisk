using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using RemoteLibrary.Channels;
using RemoteLibrary.Messages;
using RemoteLibrary.Messages.Results;
using RemoteLibrary.Messages.Values;
using RemoteLibrary.Serialization;
using RemoteLibrary.Util.Cached;
using Shared;

namespace RemoteLibrary.InterfaceProviders
{
    public class DynamicRemoteInterfaceObject : DynamicObject
    {
        private readonly IGuidProvider _guidProvider;
        private readonly Type _interfaceType;
        private readonly IRemoteInterfaceChannel _invocationChannel;
        private readonly IRemoteInterfaceSerializer _serializer;
        private readonly ICachedTypeInfoResolver _typeInfoResolver;

        public DynamicRemoteInterfaceObject(IRemoteInterfaceChannel remoteChannel,
            ICachedTypeInfoResolver typeInfoResolver, Type type, IRemoteInterfaceSerializer serializer,
            IGuidProvider guidProvider)
        {
            if (!type.IsInterface) throw new ArgumentException("Type must be an interface");

            _interfaceType = type;
            _serializer = serializer;
            _guidProvider = guidProvider;
            _invocationChannel = remoteChannel;
            _typeInfoResolver = typeInfoResolver;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            var invokeName = binder.Name;
            var invokeSuccess = false;
            var propGetMethod = _typeInfoResolver.GetTypePropertyMethodByName(_interfaceType, binder.Name, true);
            var method = _typeInfoResolver.GetMethodByName(_interfaceType, binder.Name);
            if (propGetMethod != null)
            {
                invokeName = propGetMethod.Name;

                invokeSuccess = TryInvokeMemberByName(invokeName, null, out result);
            }
            else if (method != null)
            {
                var defaultParams = method.GetParameters();
                var args = new object[defaultParams.Length];
                for (var i = 0; i < defaultParams.Length; i++)
                {
                    var param = defaultParams[i];
                    if (param.HasDefaultValue && (param.DefaultValue != null))
                    {
                        args[i] = param.DefaultValue;
                    }
                    else
                    {
                        args[i] = new NullRemoteInvocationValue(param.ParameterType);
                    }
                }
                try
                {
                    invokeSuccess = TryInvokeMemberByName(invokeName, args, out result);
                }
                catch (Exception)
                {
                    Debugger.Break();
                }
            }
            else
            {
                Debugger.Break();
            }


            return invokeSuccess || base.TryGetMember(binder, out result);
        }


        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var memberName = binder.Name;
            var method = _typeInfoResolver.GetMethodByName(_interfaceType, binder.Name);
            if (method != null)
            {
                var defaultParams = method.GetParameters();
                for (var i = 0; i < defaultParams.Length; i++)
                {
                    var param = defaultParams[i];
                    if (param.HasDefaultValue && (param.DefaultValue == args[i]) && (args[i] == null))
                    {
                        args[i] = new NullRemoteInvocationValue(param.ParameterType);
                    }
                }
            }
            var invokeSuccess = TryInvokeMemberByName(memberName, args, out result);
            return invokeSuccess || base.TryInvokeMember(binder, args, out result);
        }

        private bool TryInvokeMemberByName(string memberName, IEnumerable<object> args, out object result)
        {
            var rethrow = false; //This method shouldn't throw expections outside of those sent by the target
            try
            {
                var argValues = _serializer.ConvertObjectsToInvocationValues(args);

                var remoteInvoke = new RemoteInvocation(memberName, _interfaceType, argValues, _guidProvider);
                var invokeTask = _invocationChannel.SendMessageAndWaitForResponse(remoteInvoke);
                var baseInvokeResult = invokeTask.Result as RemoteInvocationResult;
                Debug.Assert(baseInvokeResult != null, "invokeResult != null");
                if (baseInvokeResult.IsNullOrVoid())
                {
                    result = null;
                    return true;
                }
                if (baseInvokeResult is NonNullRemoteInvocationResult)
                {
                    var invokeResult = baseInvokeResult as NonNullRemoteInvocationResult;
                    var returnType = invokeResult.ReturnType;
                    var returnValue = invokeResult.ReturnValue;

                    Debug.Assert(remoteInvoke.InvocationType != typeof(void), "Invocation result value non null for void method");
                    Debug.Assert(returnValue != null,"Invocation result value null, but defined as non null");

                    result = _serializer.DeserializeArgumentToObject(returnValue);

                    return TryHandleTaskResult(returnType, ref result);
                }
                if (baseInvokeResult is RemoteInvocationExceptionResult)
                {
                    var invokeResult = baseInvokeResult as RemoteInvocationExceptionResult;

                    rethrow = true;
                    var invokeException =
                        _serializer.DeserializeArgumentToObject(invokeResult.ExceptionValue) as Exception;
                    Debug.Assert(invokeException != null, "invokeException != null");
                    throw invokeException;
                }
            }
            catch (Exception)
            {
                if (rethrow) throw;
            }
            result = null;
            return false;
        }

        private static bool TryHandleTaskResult(Type taskType, ref object result)
        {
            var isTask = typeof (Task).IsGenericAssignableFrom(taskType);

            if (!isTask) return true;

            var returnsGenericTask = typeof (Task<>).IsGenericAssignableFrom(taskType);

            if (!returnsGenericTask) return true;


            var typeOfTask = typeof (Task);

            const string resultMethodName = "FromResult";

            var taskMethod = typeOfTask.GetMethods()
                .First(info => info.Name.Contains(resultMethodName));

            Debug.Assert(taskMethod != null, "Task creation method is null");

            var taskGenericType = taskType.GenericTypeArguments[0];

            taskMethod = taskMethod.MakeGenericMethod(new[] {taskGenericType});

            var resultTask = taskMethod.Invoke(null, new[] {result});

            result = resultTask;
            return true;
        }
    }
}