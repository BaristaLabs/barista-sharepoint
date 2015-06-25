namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;

  internal class ByteValueWriter : IValueWriter
  {
    public Type Handles
    {
      get { return typeof (byte); }
    }

    public string Write(object value)
    {
      var byteValue = (byte) value;

      return byteValue.ToString("X");
    }
  }
}