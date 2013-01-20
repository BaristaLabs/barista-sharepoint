namespace Barista.Linq2Rest.Provider.Writers
{
  using System;

  internal abstract class IntegerValueWriter : IValueWriter
  {
    public abstract Type Handles
    {
      get;
    }

    public string Write(object value)
    {
      return value.ToString();
    }
  }
}