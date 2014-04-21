using System;

namespace RemoteLibrary.Exceptions
{
    internal class RemoteInvocationException : Exception
    {
        public RemoteInvocationException()
        {
        }

        public RemoteInvocationException(string message) : base(message)
        {
        }

        public RemoteInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}