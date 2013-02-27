namespace Barista.DocumentStore
{
  using System;

  public class BaristaDocumentStoreException : BaristaException
  {
    public BaristaDocumentStoreException()
    {
    }

    public BaristaDocumentStoreException(string message)
        : base(message)
    {
    }

    public BaristaDocumentStoreException(string message, Exception inner)
        : base(message, inner)
    {
    }
  }
}
