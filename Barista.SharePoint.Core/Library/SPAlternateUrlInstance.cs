namespace Barista.SharePoint.Library
{
  using System.Workflow.ComponentModel;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPAlternateUrlConstructor : ClrFunction
  {
    public SPAlternateUrlConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPAlternateUrl", new SPAlternateUrlInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPAlternateUrlInstance Construct()
    {
      return new SPAlternateUrlInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPAlternateUrlInstance : ObjectInstance
  {
    private readonly SPAlternateUrl m_alternateUrl;

    public SPAlternateUrlInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPAlternateUrlInstance(ObjectInstance prototype, SPAlternateUrl alternateUrl)
      : this(prototype)
    {
      if (alternateUrl == null)
        throw new ArgumentNullException("alternateUrl");

      m_alternateUrl = alternateUrl;
    }

    public SPAlternateUrl SPAlternateUrl
    {
      get { return m_alternateUrl; }
    }

    [JSProperty(Name = "contextUri")]
    public UriInstance ContextUri
    {
      get
      {
        if (SPAlternateUrl.ContextUri == null)
          return null;

        return new UriInstance(this.Engine.Object.InstancePrototype, SPAlternateUrl.ContextUri);
      }
    }

    [JSProperty(Name = "incomingUrl")]
    public string IncomingUrl
    {
      get { return m_alternateUrl.IncomingUrl; }
    }

    [JSProperty(Name = "uri")]
    public UriInstance Uri
    {
      get
      {
        if (m_alternateUrl.Uri == null)
          return null;

        return new UriInstance(this.Engine.Object.InstancePrototype, m_alternateUrl.Uri);
      }
    }

    [JSProperty(Name = "urlZone")]
    public string UrlZone
    {
      get
      {
        return m_alternateUrl.UrlZone.ToString();
      }
    }
  }
}
