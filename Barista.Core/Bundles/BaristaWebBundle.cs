namespace Barista.Bundles
{
  using Barista.Library;

  public class BaristaWebBundle : WebBundleBase
  {
    protected override WebInstanceBase CreateWebInstance(Jurassic.ScriptEngine engine)
    {
      return this.WebInstance ?? (this.WebInstance = new BaristaWebInstance(engine));
    }
  }
}
