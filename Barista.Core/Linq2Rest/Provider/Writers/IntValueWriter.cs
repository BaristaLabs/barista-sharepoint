namespace Barista.Linq2Rest.Provider.Writers
{
  using System;

  internal class IntValueWriter : IntegerValueWriter
  {
    public override Type Handles
    {
      get { return typeof (int); }
    }
  }
}