namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.Web.Administration;
  using System;

  [Serializable]
  public class ApplicationPoolConstructor : ClrFunction
  {
    public ApplicationPoolConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ApplicationPool", new ApplicationPoolInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ApplicationPoolInstance Construct()
    {
      return new ApplicationPoolInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ApplicationPoolInstance : ObjectInstance
  {
    private readonly ApplicationPool m_applicationPool;

    public ApplicationPoolInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ApplicationPoolInstance(ObjectInstance prototype, ApplicationPool applicationPool)
      : this(prototype)
    {
      if (applicationPool == null)
        throw new ArgumentNullException("applicationPool");

      m_applicationPool = applicationPool;
    }

    public ApplicationPool ApplicationPool
    {
      get { return m_applicationPool; }
    }

    [JSProperty(Name = "autoStart")]
    public bool AutoStart
    {
      get { return m_applicationPool.AutoStart; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_applicationPool.Name; }
    }

    [JSProperty(Name = "state")]
    public string State
    {
      get { return m_applicationPool.State.ToString(); }
    }

    [JSProperty(Name = "managedRuntimeVersion")]
    public string ManagedRuntimeVersion
    {
      get { return m_applicationPool.ManagedRuntimeVersion; }
    }

    [JSFunction(Name = "start")]
    public string Start()
    {
      var state = m_applicationPool.Start();
      return state.ToString();
    }

    [JSFunction(Name = "stop")]
    public string Stop()
    {
      var state = m_applicationPool.Stop();
      return state.ToString();
    }

    [JSFunction(Name = "recycle")]
    public string Recycle()
    {
      var state = m_applicationPool.Recycle();
      return state.ToString();
    }
    
  }
}
