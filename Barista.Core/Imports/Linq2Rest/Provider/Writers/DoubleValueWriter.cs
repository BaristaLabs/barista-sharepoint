namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;

  internal class DoubleValueWriter : RationalValueWriter
  {
    public override Type Handles
    {
      get { return typeof (double); }
    }
  }
}