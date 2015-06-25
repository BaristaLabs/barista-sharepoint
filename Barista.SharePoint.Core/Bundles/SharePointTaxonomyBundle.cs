namespace Barista.SharePoint.Bundles
{
  using Barista.Jurassic;
  using Barista.SharePoint.Taxonomy.Library;
  using System;

  [Serializable]
  public class SharePointTaxonomyBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "SharePoint Taxonomy"; }
    }

    public string BundleDescription
    {
      get { return "SharePoint Taxonomy Bundle. Includes basic functionality for enterprise metadata management. Examples include types for managing terms, term sets, groups, keywords, term stores, and metadata service applications.\r\n This Bundle adds a top-level prototype named 'TaxonomySession' which can be instantiated with a site."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.SetGlobalValue("TaxonomySession", new TaxonomySessionConstructor(engine));

      return Null.Value;
    }
  }
}
