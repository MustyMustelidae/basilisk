using System;
using System.Collections.Generic;
using System.Reflection;

namespace RemoteLibrary.Util.Cached
{
    public interface ICachedTypeInfoResolver
    {
        MethodInfo GetMethodByName(Type type, string methodName, bool ignoreCase = false);

        PropertyInfo GetTypePropertyInfoByName(Type type, string propertyName, bool setters, bool getters,
            bool ignoreCase = false);

        IEnumerable<MethodInfo> GetTypeMethods(Type type);
        IEnumerable<MethodInfo> GetTypePropertyMethods(Type type, bool getters, bool setters);

        MethodInfo GetTypePropertyMethodByName(Type type, string propertyName, bool isGetter,
            bool ignoreCase = false);

        IEnumerable<PropertyInfo> GetTypeProperties(Type type, bool getters, bool setters);
        void RegisterIfUnregistered(Type type);
    }
}