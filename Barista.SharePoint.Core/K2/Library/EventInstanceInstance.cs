namespace Barista.SharePoint.K2.Library
{
  using Barista.SharePoint.K2Services;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class EventInstanceConstructor : ClrFunction
  {
    public EventInstanceConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "EventInstance", new EventInstanceInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public EventInstanceInstance Construct()
    {
      return new EventInstanceInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class EventInstanceInstance : ObjectInstance
  {
    private readonly EventInstance m_eventInstance;

    public EventInstanceInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public EventInstanceInstance(ObjectInstance prototype, EventInstance eventInstance)
      : this(prototype)
    {
      if (eventInstance == null)
        throw new ArgumentNullException("eventInstance");

      m_eventInstance = eventInstance;
    }

    public EventInstance EventInstance
    {
      get { return m_eventInstance; }
    }
  }
}
