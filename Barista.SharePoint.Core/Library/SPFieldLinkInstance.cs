namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint;
  using System;

  [Serializable]
  public class SPFieldLinkConstructor : ClrFunction
  {
    public SPFieldLinkConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPFieldLink", new SPFieldLinkInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPFieldLinkInstance Construct(SPFieldInstance field)
    {
      if (field == null)
        throw new JavaScriptException(this.Engine, "Error", "When constructing a new instance of a field link, a field must be supplied as the first argument.");

      var newFieldLink = new SPFieldLink(field.SPField);
      return new SPFieldLinkInstance(this.InstancePrototype, newFieldLink);
    }
  }

  [Serializable]
  public class SPFieldLinkInstance : ObjectInstance
  {
    private readonly SPFieldLink m_fieldLink;

    public SPFieldLinkInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFieldLinkInstance(ObjectInstance prototype, SPFieldLink fieldLink)
      : this(prototype)
    {
      if (fieldLink == null)
        throw new ArgumentNullException("fieldLink");

      m_fieldLink = fieldLink;
    }

    public SPFieldLink SPFieldLink
    {
      get { return m_fieldLink; }
    }

    [JSProperty(Name = "aggregationFunction")]
    public string AggregationFunction
    {
      get
      {
        return m_fieldLink.AggregationFunction;
      }
      set
      {
        m_fieldLink.AggregationFunction = value;
      }
    }

    [JSProperty(Name = "customization")]
    public string Customization
    {
      get
      {
        return m_fieldLink.Customization;
      }
      set
      {
        m_fieldLink.Customization = value;
      }
    }

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get
      {
        return m_fieldLink.DisplayName;
      }
      set
      {
        m_fieldLink.DisplayName = value;
      }
    }

    [JSProperty(Name = "hidden")]
    public bool Hidden
    {
      get
      {
        return m_fieldLink.Hidden;
      }
      set
      {
        m_fieldLink.Hidden = value;
      }
    }

    [JSProperty(Name = "Id")]
    public GuidInstance Id
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_fieldLink.Id);
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get
      {
        return m_fieldLink.Name;
      }
    }

    [JSProperty(Name = "piAttribute")]
    public string PiAttribute
    {
      get
      {
        return m_fieldLink.PIAttribute;
      }
      set
      {
        m_fieldLink.PIAttribute = value;
      }
    }

    [JSProperty(Name = "piTarget")]
    public string PiTarget
    {
      get
      {
        return m_fieldLink.PITarget;
      }
      set
      {
        m_fieldLink.PITarget = value;
      }
    }

    //Preview Value Typed

    [JSProperty(Name = "primaryPIAttribute")]
    public string PrimaryPiAttribute
    {
      get
      {
        return m_fieldLink.PrimaryPIAttribute;
      }
      set
      {
        m_fieldLink.PrimaryPIAttribute = value;
      }
    }

    [JSProperty(Name = "primaryPITarget")]
    public string PrimaryPiTarget
    {
      get
      {
        return m_fieldLink.PrimaryPITarget;
      }
      set
      {
        m_fieldLink.PrimaryPITarget = value;
      }
    }

    [JSProperty(Name = "readOnly")]
    public bool ReadOnly
    {
      get
      {
        return m_fieldLink.ReadOnly;
      }
      set
      {
        m_fieldLink.ReadOnly = value;
      }
    }

    [JSProperty(Name = "required")]
    public bool Required
    {
      get
      {
        return m_fieldLink.Required;
      }
      set
      {
        m_fieldLink.Required = value;
      }
    }

    [JSProperty(Name = "schemaXml")]
    public string SchemaXml
    {
      get
      {
        return m_fieldLink.SchemaXml;
      }
    }

    [JSProperty(Name = "showInDisplayForm")]
    public bool ShowInDisplayForm
    {
      get
      {
        return m_fieldLink.ShowInDisplayForm;
      }
      set
      {
        m_fieldLink.ShowInDisplayForm = value;
      }
    }

    [JSProperty(Name = "xPath")]
    public string XPath
    {
      get
      {
        return m_fieldLink.XPath;
      }
      set
      {
        m_fieldLink.XPath = value;
      }
    }
  }
}
