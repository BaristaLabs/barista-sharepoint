namespace Barista.SharePoint
{
  using Barista.SharePoint.Bundles;
  using Microsoft.SharePoint;
  using System;

  [Serializable]
  public sealed class SPBaristaContext : BaristaContext
  {
    private SPBaristaContext()
    {
    }

    /// <summary>
    /// Creates a new Barista context using the specified site and web.
    /// </summary>
    /// <param name="site"></param>
    /// <param name="web"></param>
    public SPBaristaContext(SPSite site, SPWeb web)
    {
      this.Site = site;
      this.Web = web;
    }

    public SPBaristaContext(BrewRequest request, BrewResponse response)
      : base(request, response)
    {
      if (request.ExtendedProperties == null || !request.ExtendedProperties.ContainsKey("SPSiteId"))
        return;

      var siteId = new Guid(request.ExtendedProperties["SPSiteId"]);

      if (siteId != Guid.Empty)
        this.Site = new SPSite(siteId);

      if (!request.ExtendedProperties.ContainsKey("SPWebId"))
        return;

      var webId = new Guid(request.ExtendedProperties["SPWebId"]);

      if (webId != Guid.Empty)
        this.Web = this.Site.OpenWeb(webId);

      if (request.ExtendedProperties.ContainsKey("SPListId"))
      {
        var listId = new Guid(request.ExtendedProperties["SPListId"]);

        if (listId != Guid.Empty)
          this.List = this.Web.Lists[listId];

        if (request.ExtendedProperties.ContainsKey("SPViewId"))
        {
          var viewId = new Guid(request.ExtendedProperties["SPViewId"]);

          if (viewId != Guid.Empty)
            this.View = this.List.Views[viewId];
        }
      }

      if (request.ExtendedProperties.ContainsKey("SPListItemUrl"))
      {
        var url = request.ExtendedProperties["SPListItemUrl"];

        if (String.IsNullOrEmpty(url) == false)
          this.ListItem = this.Web.GetListItem(url);
      }

      if (!request.ExtendedProperties.ContainsKey("SPFileId"))
        return;

      var fileId = new Guid(request.ExtendedProperties["SPFileId"]);

      if (fileId != Guid.Empty)
        this.File = this.Web.GetFile(fileId);
    }

    public SPWebBundle WebBundle
    {
      get;
      set;
    }

    public SPFile File
    {
      get;
      set;
    }

    public SPSite Site
    {
      get;
      set;
    }

    public SPWeb Web
    {
      get;
      set;
    }

    public SPList List
    {
      get;
      set;
    }

    public SPListItem ListItem
    {
      get;
      set;
    }

    public SPView View
    {
      get;
      set;
    }

    protected override void Dispose(bool disposing)
    {

      if (this.Web != null)
      {
        this.Web.Close();
        this.Web.Dispose();
        this.Web = null;
      }

      if (this.Site != null)
      {
        this.Site.Close();
        this.Site.Dispose();
        this.Site = null;
      }

      this.List = null;
      this.ListItem = null;

      base.Dispose(disposing);
    }

    #region Static Members
    [ThreadStatic]
    private static SPBaristaContext s_currentContext;

    /// <summary>
    /// Gets or sets the current Barista Context. If there is no current context one will be created using the current SPContext.
    /// </summary>
    public new static SPBaristaContext Current
    {
      get
      {
        if (s_currentContext == null && SPContext.Current != null)
        {
          s_currentContext = SPBaristaContext.CreateContextFromSPContext(SPContext.Current);
        }
        return s_currentContext;
      }
      set { s_currentContext = value; }
    }

    /// <summary>
    /// Gets a value that indicates if a current Barista context is available.
    /// </summary>
    public new static bool HasCurrentContext
    {
      get { return s_currentContext != null; }
    }

    /// <summary>
    /// Using the specified SPContext, returns a new Barista Context.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static SPBaristaContext CreateContextFromSPContext(SPContext context)
    {
      var result = new SPBaristaContext();

      if (context.Site != null)
        result.Site = new SPSite(context.Site.ID);

      if (context.Web != null)
        result.Web = result.Site.OpenWeb(context.Web.ID);

      try
      {
        if (context.List != null)
          result.List = result.Web.Lists[context.ListId];
      }
      catch (NullReferenceException) { /* Do Nothing */ }

      try
      {
        if (context.ListItem != null)
          result.ListItem = result.Web.GetListItem(context.ListItem.Url);
      }
      catch (NullReferenceException) { /* Do Nothing */ }

      return result;
    }
    #endregion

  }
}
