namespace Barista.Linq2Rest.Provider.Writers
{
  using System;

  internal interface IValueWriter
  {
    Type Handles
    {
      get;
    }

    string Write(object value);
  }

  internal abstract class ValueWriterContracts : IValueWriter
  {
    public Type Handles
    {
      get { throw new NotImplementedException(); }
    }

    public string Write(object value)
    {
      if (value == null)
        throw new ArgumentNullException("value");

      if (Handles.IsInstanceOfType(value) == false)
        throw new ArgumentOutOfRangeException("value");

      throw new NotImplementedException();
    }
  }
}