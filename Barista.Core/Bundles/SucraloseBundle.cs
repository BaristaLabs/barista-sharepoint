namespace Barista.Bundles
{
  using Barista.Jurassic;
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
      get { return "Sucralose Bundle. Includes a library that extends native objects with helpful methods similar to Sugar, however this bundle is implemented with native code."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Number.InstancePrototype.SetPropertyValue("round", new Sucralose.RoundFunctionInstance(engine, engine.Object.InstancePrototype), false);
      engine.Number.InstancePrototype.SetPropertyValue("ceil", new Sucralose.CeilFunctionInstance(engine, engine.Object.InstancePrototype), false);
      engine.Number.InstancePrototype.SetPropertyValue("floor", new Sucralose.FloorFunctionInstance(engine, engine.Object.InstancePrototype), false);


      engine.Object.SetPropertyValue("merge", new Sucralose.MergeFunctionInstance(engine, engine.Object.InstancePrototype), false);


      return Null.Value;
    }

    
  }
}
