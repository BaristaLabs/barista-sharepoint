namespace Barista.SharePoint.Publishing.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.SharePoint.Library;
  using Microsoft.SharePoint.Publishing;

  [Serializable]
  public class PublishingWebConstructor : ClrFunction
  {
    public PublishingWebConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "PublishingWeb", new PublishingWebInstance(engine.Object.InstancePrototype))
    {
      this.PopulateFields();
    }

    [JSConstructorFunction]
    public PublishingWebInstance Construct()
    {
      return new PublishingWebInstance(this.InstancePrototype);
    }

    [JSFunction(Name = "isPublishingWeb")]
    public bool IsPublishingWeb(SPWebInstance web)
    {
      if (web == null)
        throw new JavaScriptException(this.Engine, "Error", "A SPWeb must be supplied as the first parameter.");

      return PublishingWeb.IsPublishingWeb(web.Web);
    }

    [JSFunction(Name = "getDefaultDefaultPageName")]
    public string GetDefaultDefaultPageName()
    {
      return PublishingWeb.DefaultDefaultPageName;
    }

    [JSFunction(Name = "getDefaultPagesListName")]
    public string DefaultPagesListName()
    {
      return PublishingWeb.DefaultPagesListName;
    }

    [JSFunction(Name = "getPagesListName")]
    public string GetPagesListName(SPWebInstance web)
    {
      if (web == null)
        throw new JavaScriptException(this.Engine, "Error", "A SPWeb must be supplied as the first parameter.");

      return PublishingWeb.GetPagesListName(web.Web);
    }
  }

  [Serializable]
  public class PublishingWebInstance : ObjectInstance
  {
    private readonly PublishingWeb m_publishingWeb;

    public PublishingWebInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public PublishingWebInstance(ObjectInstance prototype, PublishingWeb publishingWeb)
      : this(prototype)
    {
      if (publishingWeb == null)
        throw new ArgumentNullException("publishingWeb");

      m_publishingWeb = publishingWeb;
    }

    public PublishingWeb PublishingWeb
    {
      get { return m_publishingWeb; }
    }
  }
}
