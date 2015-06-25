namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;

  internal class UnsignedLongValueWriter : IntegerValueWriter
  {
    public override Type Handles
    {
      get { return typeof (ulong); }
    }
  }
}