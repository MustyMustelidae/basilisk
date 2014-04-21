using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImpromptuInterface;

namespace RemoteLibrary.Util.Cached
{
    public class CachedMethodInvoker : ICachedMethodInvoker
    {
        private static readonly List<MethodInvoke> InvocationList = new List<MethodInvoke>();
        private static readonly object InvocationListLock = new object();


        public object Invoke(Type type, object target, MethodInfo methodInfo, object[] args)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (methodInfo == null) throw new ArgumentNullException("methodInfo");
            if (!methodInfo.IsStatic)
            {
                if (target == null)
                    throw new ArgumentNullException("target", "Value cannot be null for non static method.");
            }
            RegisterMethodIfUnregistered(type, target, methodInfo);
            var invocation = GetInvokeByMethodInfo(type, target, methodInfo);
            return invocation.Invoke(args);
        }

        private static void RegisterMethodIfUnregistered(Type type, object target, MethodInfo methodInfo)
        {
            if (IsMethodRegistered(type, target, methodInfo)) return;
            RegisterMethod(type, target, methodInfo);
        }

        private static void RegisterMethod(Type type, object target, MethodInfo methodInfo)
        {
            lock (InvocationListLock)
            {
                if (IsMethodRegistered(type, target, methodInfo)) return;
                var invoke = target == null
                    ? MethodInvoke.FromMethodInfo(type, methodInfo)
                    : MethodInvoke.FromMethodInfo(type, target, methodInfo);
                InvocationList.Add(invoke);
            }
        }

        private static MethodInvoke GetInvokeByMethodInfo(Type type, object target, MethodInfo methodInfo)
        {
            lock (InvocationListLock)
            {
                var invokeCompFunc = new Func<MethodInvoke, bool>(
                    invoke => (type == invoke.Type)
                              && invoke.MethodInfo.Equals(methodInfo)
                              && (invoke.Target == target));
                return InvocationList.FirstOrDefault(invokeCompFunc);
            }
        }

        private static bool IsMethodRegistered(Type type, object target, MethodInfo methodInfo)
        {
            lock (InvocationListLock)
            {
                var invokeCompFunc = new Func<MethodInvoke, bool>(
                    invoke => (type == invoke.Type)
                              && invoke.MethodInfo.Equals(methodInfo)
                              && (invoke.Target == target));
                return InvocationList.Any(invokeCompFunc);
            }
        }

        private struct MethodInvoke
        {
            public MethodInfo MethodInfo;


            public object Target;


            public Type Type;

            private bool _beenInvoked;

            private Delegate _methodDelegate;

            private bool IsStatic
            {
                get { return Target != null; }
            }


            public static MethodInvoke FromMethodInfo(Type type, object target, MethodInfo methodInfo)
            {
                return new MethodInvoke
                {
                    MethodInfo = methodInfo,
                    _beenInvoked = false,
                    Target = target,
                    Type = type
                };
            }

            public static MethodInvoke FromMethodInfo(Type type, MethodInfo methodInfo)
            {
                return new MethodInvoke
                {
                    MethodInfo = methodInfo,
                    _beenInvoked = false,
                    Target = null,
                    Type = type
                };
            }


            public object Invoke(object[] args)
            {
                if (_methodDelegate != null)
                {
                    //Fastest way to invoke once delegate has been created
                    return _methodDelegate.FastDynamicInvoke(args);
                }
                if (_beenInvoked)
                {
                    //Delegate creation is relatively expensive so first invocation does not trigger it
                    _methodDelegate = IsStatic
                        ? Delegate.CreateDelegate(Type, MethodInfo)
                        : Delegate.CreateDelegate(Type, Target, MethodInfo.Name);

                    return _methodDelegate.FastDynamicInvoke(args);
                }
                _beenInvoked = true;
                //Slowest method of invocation

                return MethodInfo.Invoke(Target, args);
            }
        }
    }
}