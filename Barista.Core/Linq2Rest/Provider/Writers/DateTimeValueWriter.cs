namespace Barista.Linq2Rest.Provider.Writers
{
  using System;
  using System.Xml;

  internal class DateTimeValueWriter : IValueWriter
  {
    public Type Handles
    {
      get { return typeof (DateTime); }
    }

    public string Write(object value)
    {
      var dateTimeValue = (DateTime) value;

#if !NETFX_CORE
      return string.Format("datetime'{0}'", XmlConvert.ToString(dateTimeValue, XmlDateTimeSerializationMode.Utc));
#else
			return string.Format("datetime'{0}'", XmlConvert.ToString(dateTimeValue.ToUniversalTime()));
#endif
    }
  }
}