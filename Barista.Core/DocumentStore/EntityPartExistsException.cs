namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  public class EntityPartExistsException : Exception
  {
    public EntityPartExistsException()
    {
    }

    public EntityPartExistsException(string message)
      : base(message)
    {
    }

    protected EntityPartExistsException(SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    {
      //Nada
    }
  }
}
