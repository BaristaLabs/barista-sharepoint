namespace Barista.Bundles
{
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
      get { return "Sucralose Bundle. Includes a library that extends native objects with helpful methods similar to Sugar, however this bundle is implemented with native code."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Array.InstancePrototype.SetPropertyValue("last", new Sucralose.ArrayLastFunctionInstance(engine, engine.Object.InstancePrototype), false);

      engine.Number.InstancePrototype.SetPropertyValue("round", new Sucralose.RoundFunctionInstance(engine, engine.Object.InstancePrototype), false);
      engine.Number.InstancePrototype.SetPropertyValue("ceil", new Sucralose.CeilFunctionInstance(engine, engine.Object.InstancePrototype), false);
      engine.Number.InstancePrototype.SetPropertyValue("floor", new Sucralose.FloorFunctionInstance(engine, engine.Object.InstancePrototype), false);
      engine.Number.InstancePrototype.SetPropertyValue("toNumber", new Sucralose.NumberToNumberFunctionInstance(engine, engine.Object.InstancePrototype), false);

      engine.Object.SetPropertyValue("has", new Sucralose.HasFunctionInstance(engine, engine.Object.InstancePrototype), false);
      engine.Object.SetPropertyValue("merge", new Sucralose.MergeFunctionInstance(engine, engine.Object.InstancePrototype), false);

      engine.String.SetPropertyValue("format", new Sucralose.StringFormatFunctionInstance(engine, engine.Object.InstancePrototype), false);
      engine.String.InstancePrototype.SetPropertyValue("last", new Sucralose.StringLastFunctionInstance(engine, engine.Object.InstancePrototype), false);

      var json = engine.GetGlobalValue("JSON") as ObjectInstance;
      if (json == null)
        throw new JavaScriptException(engine, "Error", "Unable to locate the JSON Object.");

      json.SetPropertyValue("parse2", new Sucralose.Parse2FunctionInstance(engine, engine.Object.InstancePrototype), false);
      json.SetPropertyValue("stringify2", new Sucralose.Stringify2FunctionInstance(engine, engine.Object.InstancePrototype), false);

      return Null.Value;
    }
  }
}
