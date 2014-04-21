using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RemoteLibrary.Util.Cached
{
    public class CachedTypeInfoResolver : ICachedTypeInfoResolver
    {
        protected static List<TypeInfo> TypeMethodList = new List<TypeInfo>();

        protected static readonly object MethodDictionaryLock = new object();

        public MethodInfo GetMethodByName(Type type, string methodName, bool ignoreCase = false)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException("methodName");
            var methods = GetTypeMethods(type);

            var nameNameComp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var methodComp = new Func<MethodInfo, bool>(info => info.Name.Equals(methodName, nameNameComp));

            return methods.FirstOrDefault(methodComp);
        }

        public PropertyInfo GetTypePropertyInfoByName(Type type, string propertyName, bool setters, bool getters,
            bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
            if (type == null) throw new ArgumentNullException("type");
            var properties = GetTypeProperties(type, getters, setters);

            var propNameComp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var propComp = new Func<PropertyInfo, bool>(info => info.Name.Equals(propertyName, propNameComp));

            return properties.FirstOrDefault(propComp);
        }

        public IEnumerable<MethodInfo> GetTypeMethods(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            RegisterIfUnregistered(type);

            var typeInfo = GetTypeInfo(type);

            return typeInfo.Methods;
        }

        public IEnumerable<MethodInfo> GetTypePropertyMethods(Type type, bool getters, bool setters)
        {
            if (type == null) throw new ArgumentNullException("type");
            RegisterIfUnregistered(type);

            var typeInfo = GetTypeInfo(type);
            var propMethods = new List<MethodInfo>();

            if (getters)
            {
                propMethods.AddRange(typeInfo.PropGetMethods.Values);
            }
            if (setters)
            {
                propMethods.AddRange(typeInfo.PropSetMethods.Values);
            }
            return propMethods;
        }

        public MethodInfo GetTypePropertyMethodByName(Type type, string propertyName, bool isGetter,
            bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
            if (type == null) throw new ArgumentNullException("type");

            RegisterIfUnregistered(type);

            var typeInfo = GetTypeInfo(type);

            var propMethods = isGetter ? typeInfo.PropGetMethods : typeInfo.PropSetMethods;
            var propNameComp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var propComp = new Func<KeyValuePair<PropertyInfo, MethodInfo>, bool>(pair =>
            {
                var currentProp = pair.Key;
                var currentPropName = currentProp.Name;
                return currentPropName.Equals(propertyName, propNameComp);
            });
            var matchingPair = propMethods.FirstOrDefault(propComp);
            Debug.Assert(!matchingPair.Equals(default(KeyValuePair<PropertyInfo, MethodInfo>)),
                "Unregistered method call searched for.");
            return propMethods.FirstOrDefault(propComp).Value;
        }

        public IEnumerable<PropertyInfo> GetTypeProperties(Type type, bool getters, bool setters)
        {
            if (type == null) throw new ArgumentNullException("type");
            RegisterIfUnregistered(type);

            var typeInfo = GetTypeInfo(type);

            return typeInfo.Properties;
        }

        public void RegisterIfUnregistered(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (!IsTypeRegistered(type)) RegisterType(type);
        }

        [Conditional("DEBUG")]
        public static void ResetTypeInfo()
        {
            lock (MethodDictionaryLock)
            {
                TypeMethodList = new List<TypeInfo>();
            }
        }

        private static TypeInfo GetTypeInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return TypeMethodList.FirstOrDefault(info => info.Type == type);
        }

        private static bool IsTypeRegistered(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            lock (MethodDictionaryLock)
            {
                return TypeMethodList.Any(info => info.Type == type);
            }
        }

        private static void RegisterType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (IsTypeRegistered(type)) return;
            var newTypeInfo = TypeInfo.FromType(type);
            RegisterType(newTypeInfo);
        }

        private static void RegisterType(TypeInfo typeInfo)
        {
            if (typeInfo.Equals(default(TypeInfo))) throw new ArgumentNullException("typeInfo");
            lock (MethodDictionaryLock)
            {
                TypeMethodList.Add(typeInfo);
            }
        }

        protected struct TypeInfo
        {
            public readonly IEnumerable<MethodInfo> Methods;
            public readonly Dictionary<PropertyInfo, MethodInfo> PropGetMethods;
            public readonly Dictionary<PropertyInfo, MethodInfo> PropSetMethods;
            public readonly IEnumerable<PropertyInfo> Properties;
            public readonly Type Type;

            private TypeInfo(Type type, IEnumerable<MethodInfo> methods,
                Dictionary<PropertyInfo, MethodInfo> propSetMethods,
                Dictionary<PropertyInfo, MethodInfo> propGetMethods, IEnumerable<PropertyInfo> properties)
            {
                Methods = methods;
                PropSetMethods = propSetMethods;
                PropGetMethods = propGetMethods;
                Properties = properties;
                Type = type;
            }

            public static TypeInfo FromType(Type type)
            {
                var methodList = type.GetMethods();
                var propSetMethodList = new Dictionary<PropertyInfo, MethodInfo>();
                var propGetMethodList = new Dictionary<PropertyInfo, MethodInfo>();
                var properties = type.GetProperties();
                foreach (var propertyInfo in properties)
                {
                    var getterInfo = propertyInfo.GetGetMethod();
                    var setterInfo = propertyInfo.GetSetMethod();
                    if (getterInfo != null)
                    {
                        propGetMethodList.Add(propertyInfo, getterInfo);
                    }
                    if (setterInfo != null)
                    {
                        propSetMethodList.Add(propertyInfo, setterInfo);
                    }
                }
                return new TypeInfo(type, methodList, propSetMethodList, propGetMethodList, properties);
            }
        }
    }
}