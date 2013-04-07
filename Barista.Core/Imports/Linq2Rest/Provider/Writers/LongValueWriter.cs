namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;

  internal class LongValueWriter : IntegerValueWriter
  {
    public override Type Handles
    {
      get { return typeof (long); }
    }
  }
}