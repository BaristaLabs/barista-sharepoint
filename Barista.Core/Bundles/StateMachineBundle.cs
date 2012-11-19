namespace Barista.Bundles
{
  using Jurassic;
  using System;

  [Serializable]
  public class StateMachineBundle : IBundle
  {
    public string BundleName
    {
      get { return "State Machine"; }
    }

    public string BundleDescription
    {
      get { return "State Machine Bundle. Provides a mechanism to create finite state machines. See https://github.com/jakesgordon/javascript-state-machine"; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(Barista.Properties.Resources.stateMachine);
      return Null.Value;
    }
  }
}
