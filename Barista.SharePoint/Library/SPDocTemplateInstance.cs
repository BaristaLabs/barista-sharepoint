namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  public class SPDocTemplateConstructor : ClrFunction
  {
    public SPDocTemplateConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPDocTemplate", new SPDocTemplateInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPDocTemplateInstance Construct(int type)
    {
      var docTemplate = BaristaContext.Current.Web.DocTemplates
                                             .OfType<SPDocTemplate>()
                                             .Where(dt => dt.Type == type)
                                             .FirstOrDefault();

      if (docTemplate == null)
        throw new JavaScriptException(this.Engine, "Error", "A document template with the specified type id does not exist on the current web.");

      return new SPDocTemplateInstance(this.InstancePrototype, docTemplate);
    }

    public SPDocTemplateInstance Construct(SPDocTemplate docTemplate)
    {
      if (docTemplate == null)
        throw new ArgumentNullException("docTemplate");

      return new SPDocTemplateInstance(this.InstancePrototype, docTemplate);
    }
  }

  public class SPDocTemplateInstance : ObjectInstance
  {
    private SPDocTemplate m_docTemplate;

    public SPDocTemplateInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPDocTemplateInstance(ObjectInstance prototype, SPDocTemplate docTemplate)
      : this(prototype)
    {
      this.m_docTemplate = docTemplate;
    }

    #region Properties

    internal SPDocTemplate DocTemplate
    {
      get { return m_docTemplate; }
    }

    [JSProperty(Name = "defaultTemplate")]
    public bool DefaultTemplate
    {
      get { return m_docTemplate.DefaultTemplate; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_docTemplate.Description; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_docTemplate.Name; }
    }

    [JSProperty(Name = "type")]
    public int Type
    {
      get { return m_docTemplate.Type; }
    }
    #endregion

    [JSFunction(Name = "getSchemaXml")]
    public string GetSchemaXml()
    {
      return m_docTemplate.SchemaXml;
    }
  }
}
