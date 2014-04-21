using System;
using System.Reflection;

namespace RemoteLibrary.Util.Cached
{
    public interface ICachedMethodInvoker
    {
        object Invoke(Type type, object target, MethodInfo methodInfo, object[] args);
    }
}