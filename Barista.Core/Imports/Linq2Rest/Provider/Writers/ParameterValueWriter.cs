namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;

  internal static class ParameterValueWriter
  {
    private static readonly IList<IValueWriter> ValueWriters;

    static ParameterValueWriter()
    {
      ValueWriters = new List<IValueWriter>
        {
          new StringValueWriter(),
          new BooleanValueWriter(),
          new IntValueWriter(),
          new LongValueWriter(),
          new ShortValueWriter(),
          new UnsignedIntValueWriter(),
          new UnsignedLongValueWriter(),
          new UnsignedShortValueWriter(),
          new ByteArrayValueWriter(),
          new StreamValueWriter(),
          new DecimalValueWriter(),
          new DoubleValueWriter(),
          new SingleValueWriter(),
          new ByteValueWriter(),
          new GuidValueWriter(),
          new DateTimeValueWriter(),
          new TimeSpanValueWriter(),
          new DateTimeOffsetValueWriter()
        };
    }

    public static string Write(object value)
    {
      if (value == null)
      {
        return "null";
      }
      var type = value.GetType();

      if (type.IsEnum)
      {
        return value.ToString();
      }

      var writer = ValueWriters.FirstOrDefault(x => x.Handles == type);

      if (writer != null)
      {
        return writer.Write(value);
      }

#if !NETFX_CORE
      if (typeof (Nullable<>).IsAssignableFrom(type))
      {
        var genericParameter = type.GetGenericArguments()[0];

        return Write(Convert.ChangeType(value, genericParameter, CultureInfo.CurrentCulture));
      }
#else
			var typeInfo = type.GetTypeInfo();
			if (typeof(Nullable<>).GetTypeInfo().IsAssignableFrom(typeInfo))
			{
				var genericParameter = typeInfo.GenericTypeArguments[0];

				return Write(Convert.ChangeType(value, genericParameter, CultureInfo.CurrentCulture));
			}
#endif

      return value.ToString();
    }
  }
}