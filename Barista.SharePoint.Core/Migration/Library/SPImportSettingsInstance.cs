namespace Barista.SharePoint.Migration.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.SharePoint.Deployment;

  [Serializable]
  public class SPImportSettingsConstructor : ClrFunction
  {
    public SPImportSettingsConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPImportSettings", new SPImportSettingsInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPImportSettingsInstance Construct()
    {
      return new SPImportSettingsInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPImportSettingsInstance : ObjectInstance
  {
    private readonly SPImportSettings m_importSettings;

    public SPImportSettingsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPImportSettingsInstance(ObjectInstance prototype, SPImportSettings importSettings)
      : this(prototype)
    {
      if (importSettings == null)
        throw new ArgumentNullException("importSettings");

      m_importSettings = importSettings;
    }

    public SPImportSettings SPImportSettings
    {
      get { return m_importSettings; }
    }
  }
}
