namespace Micron.SqlClient
{
    [System.Serializable]
    public class MicronException : System.Exception
    {
        public MicronException() { }
        public MicronException(string message) : base(message) { }
        public MicronException(string message, System.Exception inner) : base(message, inner) { }
        protected MicronException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}