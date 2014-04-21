using System;

namespace RemoteLibrary.Exceptions
{
    public class InstanceNotDefinedException : Exception
    {
        private const string ExceptionMessage = "Instance not defined for interface type {0}";

        public InstanceNotDefinedException(Type interfaceType)
            : this(interfaceType, null)
        {
        }

        public InstanceNotDefinedException(Type interfaceType, Exception innerException)
            : base(string.Format(ExceptionMessage, interfaceType.Name), innerException)
        {
        }
    }
}