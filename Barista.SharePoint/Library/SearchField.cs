namespace Barista.SharePoint.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Documents;
  using System;

  [Serializable]
  public class SearchFieldConstructor : ClrFunction
  {
    public SearchFieldConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SearchField", new SearchFieldInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SearchFieldInstance Construct()
    {
      return new SearchFieldInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SearchFieldInstance : ObjectInstance
  {
    private readonly Field m_field;

    public SearchFieldInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SearchFieldInstance(ObjectInstance prototype, Field field)
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
      get { return m_field.Name; }
    }

    [JSProperty(Name = "value")]
    public string Value
    {
      get { return m_field.StringValue; }
    }
  }
}
