namespace Barista.Linq2Rest.Provider.Writers
{
  using System;

  internal class ByteArrayValueWriter : IValueWriter
  {
    public Type Handles
    {
      get { return typeof (byte[]); }
    }

    public string Write(object value)
    {
      var base64 = Convert.ToBase64String((byte[]) value);
      return string.Format("X'{0}'", base64);
    }
  }
}