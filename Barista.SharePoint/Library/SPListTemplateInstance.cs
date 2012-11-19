namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPListTemplateConstructor : ClrFunction
  {
    public SPListTemplateConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPListTemplate", new SPListTemplateInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPListTemplateInstance Construct(string listTemplateName)
    {
      var template = BaristaContext.Current.Web.ListTemplates.OfType<SPListTemplate>()
                                           .Where( wt => wt.Name == listTemplateName)
                                           .FirstOrDefault();
 
      if (template == null)
        throw new JavaScriptException(this.Engine, "Error", "A list template with the specified name does not exist in the current web.");

      return new SPListTemplateInstance(this.InstancePrototype, template);
    }

    public SPListTemplateInstance Construct(SPListTemplate listTemplate)
    {
      if (listTemplate == null)
        throw new ArgumentNullException("listTemplate");

      return new SPListTemplateInstance(this.InstancePrototype, listTemplate);
    }
  }

  [Serializable]
  public class SPListTemplateInstance : ObjectInstance
  {
    private SPListTemplate m_listTemplate;

    public SPListTemplateInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPListTemplateInstance(ObjectInstance prototype, SPListTemplate listTemplate)
      : this(prototype)
    {
      this.m_listTemplate = listTemplate;
    }

    #region Properties
    internal SPListTemplate ListTemplate
    {
      get { return m_listTemplate; }
    }

    [JSProperty(Name = "allowsFolderCreation")]
    public bool AllowsFolderCreation
    {
      get { return m_listTemplate.AllowsFolderCreation; }
    }

    [JSProperty(Name = "baseType")]
    public string BaseType
    {
      get { return m_listTemplate.BaseType.ToString(); }
    }

    [JSProperty(Name = "categoryType")]
    public string CategoryType
    {
      get { return m_listTemplate.CategoryType.ToString(); }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_listTemplate.Description; }
    }

    [JSProperty(Name = "documentTemplate")]
    public string DocumentTemplate
    {
      get { return m_listTemplate.DocumentTemplate; }
    }

    [JSProperty(Name = "editPage")]
    public string EditPage
    {
      get { return m_listTemplate.EditPage; }
    }

    [JSProperty(Name = "featureId")]
    public string FeatureId
    {
      get { return m_listTemplate.FeatureId.ToString(); }
    }

    [JSProperty(Name = "hidden")]
    public bool Hidden
    {
      get { return m_listTemplate.Hidden; }
    }

    [JSProperty(Name = "imageUrl")]
    public string ImageUrl
    {
      get { return m_listTemplate.ImageUrl; }
    }

    [JSProperty(Name = "internalName")]
    public string InternalName
    {
      get { return m_listTemplate.InternalName; }
    }

    [JSProperty(Name = "isCustomTemplate")]
    public bool IsCustomTemplate
    {
      get { return m_listTemplate.IsCustomTemplate; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_listTemplate.Name; }
    }

    [JSProperty(Name = "newPage")]
    public string NewPage
    {
      get { return m_listTemplate.NewPage; }
    }

    [JSProperty(Name = "onQuickLaunch")]
    public bool OnQuickLaunch
    {
      get { return m_listTemplate.OnQuickLaunch; }
    }

    [JSProperty(Name = "type")]
    public string Type
    {
      get { return m_listTemplate.Type.ToString(); }
    }

    [JSProperty(Name = "type_Client")]
    public int Type_Client
    {
      get { return m_listTemplate.Type_Client; }
    }

    [JSProperty(Name = "unique")]
    public bool Unique
    {
      get { return m_listTemplate.Unique; }
    }
    #endregion

    [JSFunction(Name = "getSchemaXml")]
    public string GetSchemaXml()
    {
      return m_listTemplate.SchemaXml;
    }
  }
}
