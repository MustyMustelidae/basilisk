using System;

namespace RemoteLibrary.Exceptions
{
    /// <summary>
    /// Thrown when a remote call is made to a type that does not have an instance defined
    /// </summary>
    [Serializable]
    public class InstanceNotDefinedException : Exception
    {
        /// <summary>
        /// The exception message
        /// </summary>
        private const string ExceptionMessage = "Instance not defined for interface type {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceNotDefinedException"/> class.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        public InstanceNotDefinedException(Type interfaceType)
            : this(interfaceType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceNotDefinedException"/> class.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="innerException">The inner exception.</param>
        public InstanceNotDefinedException(Type interfaceType, Exception innerException)
            : base(string.Format(ExceptionMessage, interfaceType.Name), innerException)
        {
        }
    }
}