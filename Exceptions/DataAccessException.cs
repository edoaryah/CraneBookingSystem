using System.Runtime.Serialization;

namespace AspnetCoreMvcFull.Exceptions
{
  [Serializable]
  public class DataAccessException : Exception
  {
    public DataAccessException() : base() { }

    public DataAccessException(string message) : base(message) { }

    public DataAccessException(string message, Exception innerException)
        : base(message, innerException) { }

    [Obsolete("This constructor is obsolete and should not be used. Use the parameterless or message constructor instead.", false)]
    protected DataAccessException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
  }
}
