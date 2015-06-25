namespace Barista.SharePoint.Library
{
  using System;
  using System.Globalization;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPWebTemplateConstructor : ClrFunction
  {
    public SPWebTemplateConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPWebTemplate", new SPWebTemplateInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPWebTemplateInstance Construct(string templateName)
    {
      var template = SPBaristaContext.Current.Site.GetWebTemplates((uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID)
                                   .OfType<SPWebTemplate>().FirstOrDefault(wt => wt.Title == templateName);
 
      if (template == null)
        throw new JavaScriptException(this.Engine, "Error", "A web template with the specified name does not exist in the current site collection.");

      return new SPWebTemplateInstance(this.InstancePrototype, template);
    }

    public SPWebTemplateInstance Construct(SPWebTemplate template)
    {
      if (template == null)
        throw new ArgumentNullException("template");

      return new SPWebTemplateInstance(this.InstancePrototype, template);
    }
  }

  [Serializable]
  public class SPWebTemplateInstance : ObjectInstance
  {
    private readonly SPWebTemplate m_webTemplate;

    public SPWebTemplateInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPWebTemplateInstance(ObjectInstance prototype, SPWebTemplate template)
      : this(prototype)
    {
      this.m_webTemplate = template;
    }

    #region Properties

    internal SPWebTemplate WebTemplate
    {
      get { return m_webTemplate; }
    }

    [JSProperty(Name = "allowGlobalFeatureAssociations")]
    public bool AllowGlobalFeatureAssociations
    {
      get { return m_webTemplate.AllowGlobalFeatureAssociations; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_webTemplate.Description; }
    }

    [JSProperty(Name = "displayCategory")]
    public string DisplayCategory
    {
      get { return m_webTemplate.DisplayCategory; }
    }

    [JSProperty(Name = "filterCategories")]
    public string FilterCategories
    {
      get { return m_webTemplate.FilterCategories; }
    }

    [JSProperty(Name = "id")]
    public int Id
    {
      get { return m_webTemplate.ID; }
    }

    [JSProperty(Name = "imageUrl")]
    public string ImageUrl
    {
      get { return m_webTemplate.ImageUrl; }
    }

    [JSProperty(Name = "isCustomTemplate")]
    public bool IsCustomTemplate
    {
      get { return m_webTemplate.IsCustomTemplate; }
    }

    [JSProperty(Name = "isHidden")]
    public bool IsHidden
    {
      get { return m_webTemplate.IsHidden; }
    }

    [JSProperty(Name = "isRootWebOnly")]
    public bool IsRootWebOnly
    {
      get { return m_webTemplate.IsRootWebOnly; }
    }

    [JSProperty(Name = "isSubWebOnly")]
    public bool IsSubWebOnly
    {
      get { return m_webTemplate.IsSubWebOnly; }
    }

    [JSProperty(Name = "lcid")]
    public string Lcid
    {
      get { return m_webTemplate.Lcid.ToString(CultureInfo.InvariantCulture); }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_webTemplate.Name; }
    }

    [JSProperty(Name = "provisionAssembly")]
    public string ProvisionAssembly
    {
      get { return m_webTemplate.ProvisionAssembly; }
    }

    [JSProperty(Name = "provisionClass")]
    public string ProvisionClass
    {
      get { return m_webTemplate.ProvisionClass; }
    }

    [JSProperty(Name = "provisionData")]
    public string ProvisionData
    {
      get { return m_webTemplate.ProvisionData; }
    }

    [JSProperty(Name = "supportsMultilingualUI")]
    public bool SupportsMultilingualUI
    {
      get { return m_webTemplate.SupportsMultilingualUI; }
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get { return m_webTemplate.Title; }
    }

    [JSProperty(Name = "visibilityFeatureDependencyId")]
    public string VisibilityFeatureDependencyId
    {
      get { return m_webTemplate.VisibilityFeatureDependencyId.ToString(); }
    }
    #endregion
  }
}
