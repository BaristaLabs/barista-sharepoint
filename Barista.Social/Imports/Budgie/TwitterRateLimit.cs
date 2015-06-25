namespace Barista.Social.Imports.Budgie
{
  public class TwitterRateLimit
  {
    internal
        TwitterRateLimit()
    {
    }

    public int Limit { get; internal set; }
    public int Remaining { get; internal set; }

    public override string ToString()
    {
      return Remaining + " / " + Limit;
    }
  }
}
