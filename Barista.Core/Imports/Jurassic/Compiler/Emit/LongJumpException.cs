namespace Barista.Jurassic.Compiler
{
  using System;

  /// <summary>
  /// Used internally to allow branching out of a finally block.
  /// </summary>
  /// <remarks> This class is only public for technical reasons.  It is not intended for use by
  /// client code. </remarks>
  public class LongJumpException : Exception
  {
    /// <summary>
    /// Creates a new LongJumpException instance.
    /// </summary>
    /// <param name="routeId"> The route ID. </param>
    public LongJumpException(int routeId)
    {
      this.RouteId = routeId;
    }

    /// <summary>
    /// Gets the route ID.
    /// </summary>
    public int RouteId
    {
      get;
      private set;
    }
  }

}
