namespace Micron
{
    using System;

    [Serializable]
    public class RootCauseException : Exception
    {
        public RootCauseException() { }
        public RootCauseException(string message) : base(message) { }
        public RootCauseException(string message, Exception inner) : base(message, inner) { }
        protected RootCauseException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }   
}
