namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;

  internal class BooleanValueWriter : IValueWriter
  {
    public Type Handles
    {
      get { return typeof (bool); }
    }

    public string Write(object value)
    {
      var boolean = (bool) value;

      return boolean ? "true" : "false";
    }
  }
}