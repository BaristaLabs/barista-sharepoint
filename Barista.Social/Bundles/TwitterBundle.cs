namespace Barista.Social.Bundles
{
  public class TwitterBundle : IBundle
  {
      public bool IsSystemBundle
      {
          get { return true; }
      }

      public string BundleName
      {
          get { return "Budgie"; }
      }

      public string BundleDescription
      {
          get { return "Provides a mechanism to access Twitter via its HTTP API."; }
      }

      public virtual object InstallBundle(Jurassic.ScriptEngine engine)
      {
          return "tweet";
      }
  }
}
