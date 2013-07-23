namespace Barista.TeamFoundationServer.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.TeamFoundation.WorkItemTracking.Client;
  using System;

  [Serializable]
  public class TfsWorkItemFieldConstructor : ClrFunction
  {
    public TfsWorkItemFieldConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TfsWorkItemField", new TfsWorkItemFieldInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TfsWorkItemFieldInstance Construct()
    {
      return new TfsWorkItemFieldInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TfsWorkItemFieldInstance : ObjectInstance
  {
    private readonly Field m_field;

    public TfsWorkItemFieldInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TfsWorkItemFieldInstance(ObjectInstance prototype, Field field)
      : this(prototype)
    {
      if (field == null)
        throw new ArgumentNullException("field");

      m_field = field;
    }

    public Field Field
    {
      get { return m_field; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get
      {
        return m_field.Name;
      }
    }
  }
}