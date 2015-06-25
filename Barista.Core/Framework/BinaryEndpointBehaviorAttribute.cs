namespace Barista.Framework
{
  using System;

  /// <summary>
  /// Behavior that when applied to a WCF endpoint configured with the Multiple Header Host Factory, adds a binary endpoint at the specified port.
  /// </summary>
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