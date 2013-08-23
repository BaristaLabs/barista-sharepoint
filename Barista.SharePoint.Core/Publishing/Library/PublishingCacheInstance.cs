namespace Barista.SharePoint.Publishing.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Barista.SharePoint.Library;
  using Microsoft.SharePoint.Publishing;
  using System;
  using System.IO;

  [Serializable]
  public class PublishingCacheConstructor : ClrFunction
  {
    public PublishingCacheConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "PublishingCache", new PublishingCacheInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public PublishingCacheInstance Construct()
    {
      return new PublishingCacheInstance(this.InstancePrototype);
    }

    [JSFunction(Name = "flushBlobCache")]
    public void FlushBlobCache(SPWebApplicationInstance webApplication)
    {
      if (webApplication == null)
        throw new JavaScriptException(this.Engine, "Error", "An instance of a Web Application object must be specified as the first argument.");

      PublishingCache.FlushBlobCache(webApplication.SPWebApplication);
    }

    [JSFunction(Name = "listCacheContents")]
    public Base64EncodedByteArrayInstance ListCacheContents(SPSiteInstance site)
    {
      if (site == null)
        throw new JavaScriptException(this.Engine, "Error", "An instance of a site object must be specified as the first argument.");

      using (var ms = new MemoryStream())
      {
        PublishingCache.ListCacheContents(ms, false, site.Site);
        var blob = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, ms.ToArray());
        return blob;
      }
    }
  }

  [Serializable]
  public class PublishingCacheInstance : ObjectInstance
  {
    private readonly PublishingCache m_publishingCache;

    public PublishingCacheInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public PublishingCacheInstance(ObjectInstance prototype, PublishingCache publishingCache)
      : this(prototype)
    {
      if (publishingCache == null)
        throw new ArgumentNullException("publishingCache");

      m_publishingCache = publishingCache;
    }

    public PublishingCache PublishingCache
    {
      get { return m_publishingCache; }
    }
  }
}
