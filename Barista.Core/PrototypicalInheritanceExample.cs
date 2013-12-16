//Example of how to implement prototypical/traditional inheritance.

namespace Barista
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using System.Reflection;

  [Serializable]
  public class InheritedObjectConstructor : ClrFunction
  {
    public InheritedObjectConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "InheritedObject", new InheritedObjectInstance(engine))
    {
    }

    [JSConstructorFunction]
    public InheritedObjectInstance Construct()
    {
      return new InheritedObjectInstance(this.Engine);
    }
  }

  public class CustomObjectInstance : ObjectInstance
  {
    public CustomObjectInstance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFunctions();
    }

    /// <summary>
    /// Called by the derived classes.
    /// </summary>
    /// <param name="prototype"></param>
    protected CustomObjectInstance(ObjectInstance prototype)
      : base(prototype)
    {
      //initialization code here.
    }

    [JSProperty(Name = "a")]
    public int Test1
    {
      get { return 5; }
    }

    [JSFunction(Name = "doIt1")]
    public int DoIt1()
    {
      return 5;
    }
  }

  public class InheritedObjectInstance : CustomObjectInstance
  {
    public InheritedObjectInstance(ScriptEngine engine)
      : base(new CustomObjectInstance(engine))
    {
      this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
    }

    [JSProperty(Name = "b")]
    public int Test2
    {
      get { return 6; }
    }

    [JSFunction(Name = "doIt2")]
    public int DoIt2()
    {
      return 6;
    }
  }
}
