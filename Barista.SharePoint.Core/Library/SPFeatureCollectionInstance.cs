namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.SharePoint;
  using Barista.Library;

  [Serializable]
  public class SPFeatureCollectionConstructor : ClrFunction
  {
    public SPFeatureCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPFeatureCollection", new SPFeatureCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPFeatureCollectionInstance Construct()
    {
      return new SPFeatureCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPFeatureCollectionInstance : ObjectInstance
  {
    private readonly SPFeatureCollection m_featureCollection;

    public SPFeatureCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFeatureCollectionInstance(ObjectInstance prototype, SPFeatureCollection featureCollection)
      : this(prototype)
    {
      if (featureCollection == null)
        throw new ArgumentNullException("featureCollection");

      m_featureCollection = featureCollection;
    }

    public SPFeatureCollection SPFeatureCollection
    {
      get { return m_featureCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_featureCollection.Count;
      }
    }

    [JSFunction(Name = "add")]
    public void Add(object featureId, object force, object featureDefinitionScope)
    {
      if (featureId == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A Feature Id must be provided as the first argument.");

      var featureGuid = GuidInstance.ConvertFromJsObjectToGuid(featureId);

      if (force == null || force == Null.Value || force == Undefined.Value)
      {
        m_featureCollection.Add(featureGuid);
        return;
      }

      if (featureDefinitionScope == null || featureDefinitionScope == Null.Value ||
          featureDefinitionScope == Undefined.Value)
      {
        m_featureCollection.Add(featureGuid, TypeConverter.ToBoolean(force));
        return;
      }

      var strFeatureDefinitionScope = TypeConverter.ToString(featureDefinitionScope);
      SPFeatureDefinitionScope scope;
      strFeatureDefinitionScope.TryParseEnum(true, SPFeatureDefinitionScope.None, out scope);

      m_featureCollection.Add(featureGuid, TypeConverter.ToBoolean(force), scope);
    }

    [JSFunction(Name = "remove")]
    public void Remove(object featureId, object force)
    {
      if (featureId == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A Feature Id must be provided as the first argument.");

      var featureGuid = GuidInstance.ConvertFromJsObjectToGuid(featureId);

      if (force == null || force == Null.Value || force == Undefined.Value)
      {
        m_featureCollection.Remove(featureGuid);
        return;
      }

      m_featureCollection.Remove(featureGuid, TypeConverter.ToBoolean(force));
    }

    [JSFunction(Name = "getFeatureById")]
    public SPFeatureInstance GetFeatureById(object featureId)
    {
      if (featureId == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A Feature Id must be provided as the first argument.");

      var featureGuid = GuidInstance.ConvertFromJsObjectToGuid(featureId);

      var result = m_featureCollection[featureGuid];
      return result == null
        ? null
        : new SPFeatureInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var feature in m_featureCollection)
      {
        ArrayInstance.Push(result, new SPFeatureInstance(this.Engine.Object.InstancePrototype, feature));
      }
      return result;
    }
  }
}
