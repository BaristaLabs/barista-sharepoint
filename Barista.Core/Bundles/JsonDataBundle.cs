namespace Barista.Bundles
{
  using Jurassic;
  using System;

  [Serializable]
  public class JsonDataBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Json Data"; }
    }

    public string BundleDescription
    {
      get { return "Json Data Bundle. Provides behavior to assist with manipulating json. Currently adds:\n" +
                   "jsonDataHandler: a mechanism to Diff/Merge Json objects.\n" +
                   "automapper: a mechanism to perform json object-to-object mapping."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(Barista.Properties.Resources.jsonDataHandler);
      engine.Execute(Barista.Properties.Resources.Automapper);
      return Null.Value;
    }
  }
}
