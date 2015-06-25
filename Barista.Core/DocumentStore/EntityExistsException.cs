namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  public class EntityExistsException : Exception
  {
    public EntityExistsException()
    {
    }

    public EntityExistsException(string message)
      : base(message)
    {
    }

    protected EntityExistsException(SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    {
      //Nada
    }
  }
}
