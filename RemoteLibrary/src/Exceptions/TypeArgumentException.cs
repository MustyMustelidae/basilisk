using System;

namespace RemoteLibrary.Exceptions
{
    public class TypeArgumentException : Exception
    {
        public TypeArgumentException(string message) : base(message)
        {
        }

        public TypeArgumentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}