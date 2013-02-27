namespace Barista
{
  using System;

  public class BaristaException : Exception
  {
    public BaristaException()
    {
    }

    public BaristaException(string message)
        : base(message)
    {
    }

    public BaristaException(string message, Exception inner)
        : base(message, inner)
    {
    }
  }
}
