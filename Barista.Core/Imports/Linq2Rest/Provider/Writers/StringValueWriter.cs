namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;

  internal class StringValueWriter : IValueWriter
  {
    public Type Handles
    {
      get { return typeof (string); }
    }

    public string Write(object value)
    {
      return string.Format("'{0}'", value);
    }
  }
}