namespace Barista.SharePoint.Bundles
{
  using Barista.Bundles;
  using Barista.Library;
  using Barista.SharePoint.Library;
  using Jurassic;
  using System;

  /// <summary>
  /// Installs the sharepoint-specific WebInstance implementation.
  /// </summary>
  [Serializable]
  public class SPWebBundle : WebBundleBase
  {
    protected override WebInstanceBase CreateWebInstance(ScriptEngine engine)
    {
      return this.WebInstance ?? (this.WebInstance = new SPBaristaWebInstance(engine));
    }
  }
}
