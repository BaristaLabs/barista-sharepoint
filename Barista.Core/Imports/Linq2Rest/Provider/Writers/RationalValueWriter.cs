namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;
  using System.Globalization;

  internal abstract class RationalValueWriter : IValueWriter
  {
    public abstract Type Handles
    {
      get;
    }

    public string Write(object value)
    {
      return string.Format(CultureInfo.InvariantCulture, "{0}", value);
    }
  }
}