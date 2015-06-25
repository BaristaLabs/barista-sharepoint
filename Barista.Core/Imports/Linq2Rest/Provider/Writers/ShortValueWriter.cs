namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;

  internal class ShortValueWriter : IntegerValueWriter
  {
    public override Type Handles
    {
      get { return typeof (short); }
    }
  }
}