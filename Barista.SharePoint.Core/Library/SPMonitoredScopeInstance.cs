namespace Barista.SharePoint.Library
{
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint.Utilities;
  using System;

  [Serializable]
  public class SPMonitoredScopeConstructor : ClrFunction
  {
    public SPMonitoredScopeConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPMonitoredScope", new SPMonitoredScopeInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPMonitoredScopeInstance Construct()
    {
      return new SPMonitoredScopeInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPMonitoredScopeInstance : ObjectInstance
  {
    private readonly SPMonitoredScope m_monitoredScope;

    public SPMonitoredScopeInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPMonitoredScopeInstance(ObjectInstance prototype, SPMonitoredScope monitoredScope)
      : this(prototype)
    {
      if (monitoredScope == null)
        throw new ArgumentNullException("monitoredScope");

      m_monitoredScope = monitoredScope;
    }

    public SPMonitoredScope SPMonitoredScope
    {
      get { return m_monitoredScope; }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get { return new GuidInstance(this.Engine.Object.Prototype, m_monitoredScope.Id); }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_monitoredScope.Name; }
    }

    [JSProperty(Name = "startTime")]
    public object StartTime
    {
      get
      {
        var counter = m_monitoredScope.GetMonitor<SPExecutionTimeCounter>();

        if (counter == null)
          return Null.Value;

        return JurassicHelper.ToJsDate(counter.StartTime);
      }
    }

    [JSProperty(Name = "elapsedTime")]
    public object ElapsedTime
    {
      get
      {
        var counter = m_monitoredScope.GetMonitor<SPExecutionTimeCounter>();

        if (counter == null)
          return Null.Value;

        return counter.ElapsedTime.ToString();
      }
    }

    [JSProperty(Name = "endTime")]
    public object EndTime
    {
      get
      {
        var counter = m_monitoredScope.GetMonitor<SPExecutionTimeCounter>();

        if (counter == null)
          return Null.Value;

        return JurassicHelper.ToJsDate(counter.EndTime);
      }
    }

    [JSFunction(Name = "dispose")]
    public void Dispose()
    {
      m_monitoredScope.Dispose();
    }
  }
}
