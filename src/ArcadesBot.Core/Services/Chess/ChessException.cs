using System;
using System.Runtime.Serialization;

namespace ArcadesBot
{
    [Serializable]
    public class ChessException : Exception
    {
        public ChessException()
        {
        }

        public ChessException(string message) : base(message)
        {
        }

        public ChessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ChessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}