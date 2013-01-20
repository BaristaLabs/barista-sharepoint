namespace Barista.Linq2Rest.Provider.Writers
{
  using System;
  using System.Xml;

  internal class DateTimeOffsetValueWriter : IValueWriter
  {
    public Type Handles
    {
      get { return typeof (DateTimeOffset); }
    }

    public string Write(object value)
    {
      return string.Format("datetimeoffset'{0}'", XmlConvert.ToString((DateTimeOffset) value));
    }
  }
}