using RavenDB = Raven;

namespace Barista.Raven.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using System.Linq;
  using Barista.Newtonsoft.Json;

  [Serializable]
  public class IndexDefinitionConstructor : ClrFunction
  {
    public IndexDefinitionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "IndexDefinition", new IndexDefinitionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public IndexDefinitionInstance Construct()
    {
      return new IndexDefinitionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class IndexDefinitionInstance : ObjectInstance
  {
    private readonly RavenDB.Abstractions.Indexing.IndexDefinition m_indexDefinition;

    public IndexDefinitionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IndexDefinitionInstance(ObjectInstance prototype, RavenDB.Abstractions.Indexing.IndexDefinition indexDefinition)
      : this(prototype)
    {
      if (indexDefinition == null)
        throw new ArgumentNullException("indexDefinition");

      m_indexDefinition = indexDefinition;
    }

    public RavenDB.Abstractions.Indexing.IndexDefinition IndexDefinition
    {
      get { return m_indexDefinition; }
    }

    //TODO: Add other properties....

    [JSProperty(Name = "fields")]
    [JsonProperty("fields")]
    public object Fields
    {
      get
      {
        if (m_indexDefinition.Fields == null)
          return Null.Value;

// ReSharper disable CoVariantArrayConversion
        var result = this.Engine.Array.Construct(m_indexDefinition.Fields.ToArray());
// ReSharper restore CoVariantArrayConversion
        return result;
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
        {
          m_indexDefinition.Fields = null;
          return;
        }

        var valueArr = value as ArrayInstance;
        if (valueArr == null)
          throw new JavaScriptException(this.Engine, "Error", "fields property value must be an array.");

        m_indexDefinition.Fields = valueArr.ElementValues.Select(ev => TypeConverter.ToString(ev)).ToList();
      }
    }

    [JSProperty(Name = "isCompiled")]
    [JsonProperty("isCompiled")]
    public bool IsCompiled
    {
      get { return m_indexDefinition.IsCompiled; }
      set { m_indexDefinition.IsCompiled = value; }
    }

    [JSProperty(Name = "isMapReduce")]
    [JsonIgnore]
    public bool IsMapReduce
    {
      get { return m_indexDefinition.IsMapReduce; }
    }

    [JSProperty(Name = "name")]
    [JsonProperty("name")]
    public string Name
    {
      get { return m_indexDefinition.Name; }
      set { m_indexDefinition.Name = value; }
    }

    [JSProperty(Name = "map")]
    [JsonProperty("map")]
    public string Map
    {
      get { return m_indexDefinition.Map; }
      set { m_indexDefinition.Map = value; }
    }

    [JSProperty(Name = "reduce")]
    [JsonProperty("reduce")]
    public string Reduce
    {
      get { return m_indexDefinition.Reduce; }
      set { m_indexDefinition.Reduce = value; }
    }

    [JSProperty(Name = "type")]
    [JsonIgnore]
    public string Type
    {
      get { return m_indexDefinition.Type; }
    }
  }
}
