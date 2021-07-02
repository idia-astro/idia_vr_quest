using System;

namespace Models
{
    public class BackendException : Exception
    {
        public BackendException(string message) : base(message)
        {
        }

        public BackendException(Error error) : base(error.message)
        {
            
        }
    }
    public struct Error
    {
        public string message;
    }
}