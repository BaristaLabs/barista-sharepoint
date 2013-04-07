namespace Barista.Imports.Linq2Rest.Provider.Writers
{
  using System;

  internal class SingleValueWriter : RationalValueWriter
  {
    public override Type Handles
    {
      get { return typeof (float); }
    }
  }
}