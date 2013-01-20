namespace Barista.Linq2Rest.Provider.Writers
{
  using System;

  internal class DecimalValueWriter : RationalValueWriter
  {
    public override Type Handles
    {
      get { return typeof (decimal); }
    }
  }
}