using System;

namespace Shared
{
    public static class TypeExtensions

    {
        public static bool IsGenericAssignableFrom(this Type baseType, Type extendType)
        {
            while (!baseType.IsAssignableFrom(extendType))
            {
                if (extendType == typeof (object))
                {
                    return false;
                }
                if (extendType == null)
                {
                    return false;
                }
                if (extendType.IsGenericType && !extendType.IsGenericTypeDefinition)
                {
                    extendType = extendType.GetGenericTypeDefinition();
                }
                else
                {
                    extendType = extendType.BaseType;
                }
            }
            return true;
        }
    }
}