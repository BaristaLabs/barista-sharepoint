namespace Barista.SharePoint.Migration.Library
{
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Newtonsoft.Json;
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

    [JSProperty(Name = "activateSolutions")]
    [JsonProperty("activateSolutions")]
    [JSDoc("Gets or sets a Boolean value to specify whether to activate user solutions upon conclusion of the import operation.")]
    public bool ActivateSolutions
    {
      get
      {
        return m_importSettings.ActivateSolutions;
      }
      set
      {
        m_importSettings.ActivateSolutions = value;
      }
    }

    [JSProperty(Name = "baseFileName")]
    [JsonProperty("baseFileName")]
    [JSDoc("Gets or sets the base file name used when creating content migration packages.")]
    public string BaseFileName
    {
      get
      {
        return m_importSettings.BaseFileName;
      }
      set
      {
        m_importSettings.BaseFileName = value;
      }
    }

    [JSProperty(Name = "commandLineVerbose")]
    [JsonProperty("commandLineVerbose")]
    [JSDoc("Gets or sets a value that determines whether output is written to a command line console.")]
    public bool CommandLineVerbose
    {
      get
      {
        return m_importSettings.CommandLineVerbose;
      }
      set
      {
        m_importSettings.CommandLineVerbose = value;
      }
    }

    [JSProperty(Name = "fileCompression")]
    [JsonProperty("fileCompression")]
    [JSDoc("Gets or sets a Boolean value that specifies whether the content migration package is compressed using the CAB compression protocol.")]
    public bool FileCompression
    {
      get
      {
        return m_importSettings.FileCompression;
      }
      set
      {
        m_importSettings.FileCompression = value;
      }
    }

    [JSProperty(Name = "fileLocation")]
    [JsonProperty("fileLocation")]
    [JSDoc(@"Gets or sets the directory location where content migration packages are placed. This value can be any valid URI, for example, http://www.MySite.com/ or \\MySite\.")]
    public string FileLocation
    {
      get
      {
        return m_importSettings.FileLocation;
      }
      set
      {
        m_importSettings.FileLocation = value;
      }
    }

    [JSProperty(Name = "haltOnNonfatalError")]
    [JsonProperty("haltOnNonfatalError")]
    public bool HaltOnNonfatalError
    {
      get
      {
        return m_importSettings.HaltOnNonfatalError;
      }
      set
      {
        m_importSettings.HaltOnNonfatalError = value;
      }
    }

    [JSProperty(Name = "haltOnWarning")]
    [JsonProperty("haltOnWarning")]
    public bool HaltOnWarning
    {
      get
      {
        return m_importSettings.HaltOnWarning;
      }
      set
      {
        m_importSettings.HaltOnWarning = value;
      }
    }

    [JSProperty(Name = "ignoreWebParts")]
    [JsonProperty("ignoreWebParts")]
    [JSDoc("Gets or sets a value that specifies whether the import operation ignores Web Parts associated with a file. Set to true to disable importing Web Parts; the default value is false.")]
    public bool IgnoreWebParts
    {
      get
      {
        return m_importSettings.IgnoreWebParts;
      }
      set
      {
        m_importSettings.IgnoreWebParts = value;
      }
    }

    [JSProperty(Name = "includeSecurity")]
    [JsonProperty("includeSecurity")]
    [JSDoc("Gets or sets a value that determines whether site security groups and the group membership information is exported or imported. Possible Values: All, None, WssOnly.")]
    public string IncludeSecurity
    {
      get
      {
        return m_importSettings.IncludeSecurity.ToString();
      }
      set
      {
        SPIncludeSecurity includeSecurity;
        if (value.TryParseEnum(true, out includeSecurity))
          m_importSettings.IncludeSecurity = includeSecurity;
      }
    }

    [JSProperty(Name = "includeUserCustomAction")]
    [JsonProperty("includeUserCustomAction")]
    [JSDoc("Gets or sets a value from the SPIncludeUserCustomAction enumeration that specifies whether to include custom user actions with the import settings. Possible Values: All, None. Guess that's all she wrote.")]
    public string IncludeUserCustomAction
    {
      get
      {
        return m_importSettings.IncludeUserCustomAction.ToString();
      }
      set
      {
        SPIncludeUserCustomAction includeUserCustomAction;
        if (value.TryParseEnum(true, out includeUserCustomAction))
          m_importSettings.IncludeUserCustomAction = includeUserCustomAction;
      }
    }
  }
}
