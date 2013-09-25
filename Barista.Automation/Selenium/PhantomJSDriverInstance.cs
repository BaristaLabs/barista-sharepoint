namespace Barista.Automation.Selenium
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OpenQA.Selenium.PhantomJS;
  using System.Linq;
  using System;

  [Serializable]
  public class PhantomJSDriverConstructor : ClrFunction
  {
    private static readonly object SyncRoot = new object();
    private static volatile PhantomJSDriverService s_defaultService;

    public PhantomJSDriverConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "PhantomJSDriver", new SearchContextInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public WebDriverInstance Construct(params object[] args)
    {
      var arg1 = args.ElementAtOrDefault(0);
      var arg2 = args.ElementAtOrDefault(1);
      var arg3 = args.ElementAtOrDefault(2);

      if ((arg1 == null || arg1 == Null.Value || arg1 == Undefined.Value) &&
          (arg2 == null || arg2 == Null.Value || arg2 == Undefined.Value) &&
          (arg3 == null || arg3 == Null.Value || arg3 == Undefined.Value))
      {
        if (s_defaultService == null)
        {
          lock (SyncRoot)
          {
            if (s_defaultService == null)
              s_defaultService = PhantomJSDriverService.CreateDefaultService();
          }
        }

        return new PhantomJSDriverInstance(this.InstancePrototype, new PhantomJSDriver(s_defaultService));
      }

      throw new NotImplementedException();
    }
  }

  [Serializable]
  public class PhantomJSDriverInstance : RemoteWebDriverInstance
  {

    public PhantomJSDriverInstance(ObjectInstance prototype, PhantomJSDriver driver)
      : base(prototype, driver)
    {
    }
  }
}
