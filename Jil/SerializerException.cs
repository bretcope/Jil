using System;

namespace StringInterningJil
{
    /// <summary>
    /// An exception thrown when Jil encounters an error while serializing an object.
    /// </summary>
    public class SerializerException : Exception
    {
        internal SerializerException(string message, Exception innerException) :
            base(message + ": " + innerException.Message, innerException)
        { }

        internal SerializerException(string message) : base(message)
        {
        }
    }
}
