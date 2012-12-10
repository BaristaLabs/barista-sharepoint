namespace Barista.Justache
{
  using System;
  //using System.Runtime.Serialization;

  [Serializable]
  public class JustacheException : Exception
  {
    //public NustacheException()
    //{
    //}

    public JustacheException(string message)
      : base(message)
    {
    }

    //public NustacheException(string message, Exception inner)
    //    : base(message, inner)
    //{
    //}

    //protected NustacheException(SerializationInfo info, StreamingContext context)
    //    : base(info, context)
    //{
    //}
  }
}