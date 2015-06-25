namespace Barista.Raven.Bundles
{
  using Barista.Jurassic;
  using Barista.Raven.Library;

  public class RavenClientBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Raven Client"; }
    }

    public string BundleDescription
    {
      get { return "Provides access to RavenDB from Barista. Adds the top-level DocumentStore object."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.SetGlobalValue("IndexQuery", new IndexQueryConstructor(engine));
      engine.SetGlobalValue("IndexDefinition", new IndexDefinitionConstructor(engine));

      engine.SetGlobalValue("DocumentStore", new DocumentStoreConstructor(engine));

      return Null.Value;
    }
  }
}
