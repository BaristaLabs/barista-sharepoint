namespace Barista.Bundles
{
  using System;
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;

  public class SucraloseBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Sucralose"; }
    }

    public string BundleDescription
    {
      get { return "Sucralose Bundle. Includes a library that extends native objects with helpful methods similar to Sugar, however this bundle is implemented with native .Net code."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Object.SetPropertyValue("merge", new Sucralose.MergeFunctionInstance(engine, engine.Object.InstancePrototype), false);


      return Null.Value;
    }

    
  }
}
