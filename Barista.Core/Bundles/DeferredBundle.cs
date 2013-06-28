namespace Barista.Bundles
{
  using Barista.Library;
  using Jurassic;
  using System;

  [Serializable]
  public class DeferredBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Deferred"; }
    }

    public string BundleDescription
    {
      get { return "Deferred Bundle. Contains functionality to perform multi-threaded, async tasks.";  }
    }

    public object InstallBundle(ScriptEngine engine)
    {
      engine.SetGlobalValue("Deferred", new DeferredConstructor(engine));

      engine.SetGlobalFunction("delay", new Action<int>((millisecondsTimeout) => { System.Threading.Thread.Sleep(millisecondsTimeout); }));
      engine.SetGlobalFunction("waitAll", new Action<object, object>((deferreds, timeout) => { DeferredInstance.WaitAll(deferreds, timeout); }));

      return Null.Value;
    }
  }
}
