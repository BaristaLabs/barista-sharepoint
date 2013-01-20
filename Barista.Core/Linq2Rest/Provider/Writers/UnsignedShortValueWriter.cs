namespace Barista.Linq2Rest.Provider.Writers
{
  using System;

  internal class UnsignedShortValueWriter : IntegerValueWriter
  {
    public override Type Handles
    {
      get { return typeof (ushort); }
    }
  }
}