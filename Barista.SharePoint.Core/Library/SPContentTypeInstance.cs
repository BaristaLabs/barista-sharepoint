namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPContentTypeConstructor : ClrFunction
  {
    public SPContentTypeConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPContentType", new SPContentTypeInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPContentTypeInstance Construct(string contentTypeId)
    {
      var spContentTypeId = new SPContentTypeId(contentTypeId);
      var bestSPContentTypeMatch = BaristaContext.Current.Web.AvailableContentTypes.BestMatch(spContentTypeId);
      var contentType = BaristaContext.Current.Web.AvailableContentTypes[bestSPContentTypeMatch];

      if (contentType == null)
        throw new JavaScriptException(this.Engine, "Error", "A match for the specified content type could not be found in the current web.");

      return new SPContentTypeInstance(this.InstancePrototype, contentType);
    }

    public SPContentTypeInstance Construct(SPContentType contentType)
    {
      if (contentType == null)
        throw new ArgumentNullException("contentType");

      return new SPContentTypeInstance(this.InstancePrototype, contentType);
    }
  }

  [Serializable]
  public class SPContentTypeInstance : ObjectInstance
  {
    private SPContentType m_contentType;

    public SPContentTypeInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPContentTypeInstance(ObjectInstance prototype, SPContentType contentType)
      : this(prototype)
    {
      this.m_contentType = contentType;
    }

    internal SPContentType ContentType
    {
      get { return m_contentType; }
    }

    #region Properties
    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_contentType.Description; }
      set { m_contentType.Description = value; }
    }

    [JSProperty(Name = "displayFormTemplateName")]
    public string DisplayFormTemplateName
    {
      get { return m_contentType.DisplayFormTemplateName; }
      set { m_contentType.DisplayFormTemplateName = value; }
    }

    [JSProperty(Name = "displayFormUrl")]
    public string DisplayFormUrl
    {
      get { return m_contentType.DisplayFormUrl; }
      set { m_contentType.DisplayFormUrl = value; }
    }

    [JSProperty(Name = "documentTemplate")]
    public string DocumentTemplate
    {
      get { return m_contentType.DocumentTemplate;  }
      set { m_contentType.DocumentTemplate = value; }
    }

    [JSProperty(Name = "documentTemplateUrl")]
    public string DocumentTemplateUrl
    {
      get { return m_contentType.DocumentTemplateUrl; }
    }

    [JSProperty(Name = "editFormTemplateName")]
    public string EditFormTemplateName
    {
      get { return m_contentType.EditFormTemplateName; }
      set { m_contentType.EditFormTemplateName = value; }
    }

    [JSProperty(Name = "editFormUrl")]
    public string EditFormUrl
    {
      get { return m_contentType.EditFormUrl; }
      set { m_contentType.EditFormUrl = value; }
    }

    //TODO: Event Receivers

    [JSProperty(Name = "featureId")]
    public string FeatureId
    {
      get { return m_contentType.FeatureId.ToString(); }
    }

    //TODO: FieldLinks, Fields

    [JSProperty(Name = "group")]
    public string Group
    {
      get { return m_contentType.Group; }
      set { m_contentType.Group = value; }
    }

    [JSProperty(Name = "hidden")]
    public bool Hidden
    {
      get { return m_contentType.Hidden; }
      set { m_contentType.Hidden = value; }
    }

    [JSProperty(Name = "id")]
    public SPContentTypeIdInstance Id
    {
      get { return new SPContentTypeIdInstance(this.Engine.Object.InstancePrototype, m_contentType.Id); }
    }

    [JSProperty(Name = "mobileDisplayFormUrl")]
    public string MobileDisplayFormUrl
    {
      get { return m_contentType.MobileDisplayFormUrl; }
      set { m_contentType.MobileDisplayFormUrl = value; }
    }

    [JSProperty(Name = "mobileEditFormUrl")]
    public string MobileEditFormUrl
    {
      get { return m_contentType.MobileEditFormUrl; }
      set { m_contentType.MobileEditFormUrl = value; }
    }

    [JSProperty(Name = "mobileNewFormUrl")]
    public string MobileNewFormUrl
    {
      get { return m_contentType.MobileNewFormUrl; }
      set { m_contentType.MobileNewFormUrl = value; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_contentType.Name; }
      set { m_contentType.Name = value; }
    }

    [JSProperty(Name = "newDocumentControl")]
    public string NewDocumentControl
    {
      get { return m_contentType.NewDocumentControl; }
      set { m_contentType.NewDocumentControl = value; }
    }

    [JSProperty(Name = "newFormTemplateName")]
    public string NewFormTemplateName
    {
      get { return m_contentType.NewFormTemplateName; }
      set { m_contentType.NewFormTemplateName = value; }
    }

    [JSProperty(Name = "newFormUrl")]
    public string NewFormUrl
    {
      get { return m_contentType.NewFormUrl; }
      set { m_contentType.NewFormUrl = value; }
    }

    [JSProperty(Name = "readOnly")]
    public bool ReadOnly
    {
      get { return m_contentType.ReadOnly; }
      set { m_contentType.ReadOnly = value; }
    }

    [JSProperty(Name = "requireClientRenderingOnNew")]
    public bool RequireClientRenderingOnNew
    {
      get { return m_contentType.RequireClientRenderingOnNew; }
      set { m_contentType.RequireClientRenderingOnNew = value; }
    }

    [JSProperty(Name = "scope")]
    public string Scope
    {
      get { return m_contentType.Scope; }
    }

    [JSProperty(Name = "sealed")]
    public bool Sealed
    {
      get { return m_contentType.Sealed; }
      set { m_contentType.Sealed = value; }
    }

    [JSProperty(Name = "version")]
    public int Version
    {
      get { return m_contentType.Version; }
    }

    //TODO: Workflow Associations and XmlDocuments.
    #endregion

    #region Functions
    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_contentType.Delete();
    }

    [JSFunction(Name = "getParent")]
    public SPContentTypeInstance GetParent()
    {
      return new SPContentTypeInstance(this.Engine.Object.InstancePrototype, m_contentType.Parent);
    }

    [JSFunction(Name = "getParentList")]
    public SPListInstance GetParentList()
    {
      return new SPListInstance(this.Engine.Object.InstancePrototype, null, null, m_contentType.ParentList);
    }

    [JSFunction(Name = "getParentWeb")]
    public SPWebInstance ParentWeb()
    {
      return new SPWebInstance(this.Engine.Object.InstancePrototype, m_contentType.ParentWeb);
    }

    [JSFunction(Name = "getResourceFolder")]
    public SPFolderInstance ResourceFolder()
    {
      return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_contentType.ResourceFolder);
    }

    [JSFunction(Name = "update")]
    public void Update([DefaultParameterValue(true)] bool updateChildren, [DefaultParameterValue(true)] bool throwOnSealedOrReadOnly)
    {
      m_contentType.Update(updateChildren, throwOnSealedOrReadOnly);
    }
    #endregion

    [JSFunction(Name = "getSchemaXml")]
    public string GetSchemaXml()
    {
      return m_contentType.SchemaXml;
    }

    [JSFunction(Name = "getSchemaXmlWithResourceTokens")]
    public string GetSchemaXmlWithResourceTokens()
    {
      return m_contentType.SchemaXmlWithResourceTokens;
    }

    [JSFunction(Name = "setSchemaXmlWithResourceTokens")]
    public void SetSchemaXmlWithResourceTokens(string xml)
    {
      m_contentType.SchemaXmlWithResourceTokens = xml;
    }
  }
}
