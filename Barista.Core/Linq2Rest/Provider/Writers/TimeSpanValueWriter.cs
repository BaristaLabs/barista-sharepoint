namespace Barista.Linq2Rest.Provider.Writers
{
  using System;
  using System.Xml;

  internal class TimeSpanValueWriter : IValueWriter
  {
    public Type Handles
    {
      get { return typeof (TimeSpan); }
    }

    public string Write(object value)
    {
      return string.Format("time'{0}'", XmlConvert.ToString((TimeSpan) value));
    }
  }
}