namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;

  internal class GuidValueWriter : IValueWriter
  {
    public Type Handles
    {
      get { return typeof (Guid); }
    }

    public string Write(object value)
    {
      return string.Format("guid'{0}'", value);
    }
  }
}