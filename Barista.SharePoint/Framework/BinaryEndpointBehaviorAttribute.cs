namespace Barista.SharePoint.Framework
{
  using System;

  public class BinaryEndpointBehaviorAttribute : Attribute
  {
    public BinaryEndpointBehaviorAttribute()
    {
      this.PortNumber = 8080;
    }

    public BinaryEndpointBehaviorAttribute(int portNumber)
    {
      this.PortNumber = portNumber;
    }

    public int PortNumber
    {
      get;
      set;
    }
  }
}
