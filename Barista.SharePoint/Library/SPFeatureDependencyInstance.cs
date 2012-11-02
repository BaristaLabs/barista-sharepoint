namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;

  public class SPFeatureDependencyConstructor : ClrFunction
  {
    public SPFeatureDependencyConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPFeatureDependency", new SPFeatureDependencyInstance(engine.Object.InstancePrototype))
    {
    }

    public SPFeatureDependencyInstance Construct(SPFeatureDependency featureDependency)
    {
      if (featureDependency == null)
        throw new ArgumentNullException("featureDependency");

      return new SPFeatureDependencyInstance(this.InstancePrototype, featureDependency);
    }
  }

  public class SPFeatureDependencyInstance : ObjectInstance
  {
    private SPFeatureDependency m_featureDependency;

    public SPFeatureDependencyInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFeatureDependencyInstance(ObjectInstance prototype, SPFeatureDependency featureDependency)
      : this(prototype)
    {
      this.m_featureDependency = featureDependency;
    }

    [JSProperty(Name = "featureId")]
    public string FeatureId
    {
      get { return m_featureDependency.FeatureId.ToString(); }
    }

    [JSProperty(Name = "minimumVersion")]
    public string MinimumVersion
    {
      get { return m_featureDependency.MinimumVersion.ToString(); }
    }
  }
}
