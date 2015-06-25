namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPFeatureConstructor : ClrFunction
  {
    public SPFeatureConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPFeature", new SPFeatureInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPFeatureInstance Construct(object featureId)
    {
      var featureGuid = GuidInstance.ConvertFromJsObjectToGuid(featureId);

      var featureQueryResult = SPBaristaContext.Current.Site.QueryFeatures(featureGuid);
      var feature = featureQueryResult.OrderByDescending(f => f.Version).FirstOrDefault();
      
      if (feature == null)
        throw new JavaScriptException(this.Engine, "Error", "A feature with the specified id is not installed in the farm.");

      return new SPFeatureInstance(this.InstancePrototype, feature);
    }

    public SPFeatureInstance Construct(SPFeature feature)
    {
      if (feature == null)
        throw new ArgumentNullException("feature");

      return new SPFeatureInstance(this.InstancePrototype, feature);
    }
  }

  [Serializable]
  public class SPFeatureInstance : ObjectInstance
  {
    private readonly SPFeature m_feature;

    public SPFeatureInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFeatureInstance(ObjectInstance prototype, SPFeature feature)
      : this(prototype)
    {
      this.m_feature = feature;
    }

    internal SPFeature Feature
    {
      get { return m_feature; }
    }

    [JSProperty(Name = "definitionId")]
    public string DefinitionId
    {
      get { return m_feature.DefinitionId.ToString(); }
    }

    [JSProperty(Name = "featureDefinitionScope")]
    public string FeatureDefinitionScope
    {
      get { return m_feature.FeatureDefinitionScope.ToString(); }
    }

    //TODO: Properties

    [JSProperty(Name = "version")]
    public string Version
    {
      get { return m_feature.Version.ToString(); }
    }

    [JSFunction(Name = "getDefinition")]
    public SPFeatureDefinitionInstance GetDefinition()
    {
      SPFeatureDefinition definition = m_feature.Definition;
      if (definition == null)
      {
        var parent = m_feature.Parent;
        if (parent is SPWeb)
          definition = (parent as SPWeb).Site.FeatureDefinitions[m_feature.DefinitionId];
        else if (parent is SPSite)
          definition = (parent as SPSite).FeatureDefinitions[m_feature.DefinitionId];
        else
          throw new InvalidOperationException("Unknown feature parent object type: " + parent.GetType());
      }

      return new SPFeatureDefinitionInstance(this.Engine.Object.InstancePrototype, definition);
    }
  }
}
