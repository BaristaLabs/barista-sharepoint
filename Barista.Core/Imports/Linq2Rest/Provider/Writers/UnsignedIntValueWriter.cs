namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;

  internal class UnsignedIntValueWriter : IntegerValueWriter
  {
    public override Type Handles
    {
      get { return typeof (uint); }
    }
  }
}