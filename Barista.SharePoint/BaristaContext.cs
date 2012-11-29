namespace Barista.SharePoint
{
  using Microsoft.SharePoint;
  using System;

  [Serializable]
  public sealed class BaristaContext : IDisposable
  {
    [ThreadStatic]
    private static BaristaContext s_currentContext;

    public static BaristaContext Current
    {
      get
      {
        if (s_currentContext == null && SPContext.Current != null)
        {
          s_currentContext = BaristaContext.CreateContextFromSPContext(SPContext.Current);
        }
        return s_currentContext;
      }
      set { s_currentContext = value; }
    }

    public static bool HasCurrentContext
    {
      get { return s_currentContext != null; }
    }

    public BaristaContext()
    {
    }

    public BaristaContext(SPSite site, SPWeb web)
    {
      this.Site = site;
      this.Web = web;
    }

    public BaristaContext(BrewRequest request, BrewResponse response)
      : this()
    {
      if (request == null)
        throw new ArgumentNullException("request");

      if (response == null)
        response = new BrewResponse();

      this.Request = request;
      this.Response = response;

      if (request.ExtendedProperties != null && request.ExtendedProperties.ContainsKey("SPSiteId"))
      {
        Guid siteId = new Guid(request.ExtendedProperties["SPSiteId"]);

        if (siteId != Guid.Empty)
          this.Site = new SPSite(siteId);

        if (request.ExtendedProperties.ContainsKey("SPWebId"))
        {
          Guid webId = new Guid(request.ExtendedProperties["SPWebId"]);

          if (webId != Guid.Empty)
            this.Web = this.Site.OpenWeb(webId);

          if (request.ExtendedProperties.ContainsKey("SPListId"))
          {
            Guid listId = new Guid(request.ExtendedProperties["SPListId"]);

            if (listId != Guid.Empty)
              this.List = this.Web.Lists[listId];

            if (request.ExtendedProperties.ContainsKey("SPViewId"))
            {
              Guid viewId = new Guid(request.ExtendedProperties["SPViewId"]);

              if (viewId != Guid.Empty)
                this.View = this.List.Views[viewId];
            }
          }

          if (request.ExtendedProperties.ContainsKey("SPListItemUrl"))
          {
            string url = request.ExtendedProperties["SPListItemUrl"];

            if (String.IsNullOrEmpty(url) == false)
              this.ListItem = this.Web.GetListItem(url);
          }

          if (request.ExtendedProperties.ContainsKey("SPFileId"))
          {
            Guid fileId = new Guid(request.ExtendedProperties["SPFileId"]);

            if (fileId != Guid.Empty)
              this.File = this.Web.GetFile(fileId);
          }
        }
      }
    }

    public BrewRequest Request
    {
      get;
      private set;
    }

    public BrewResponse Response
    {
      get;
      private set;
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

    public static BaristaContext CreateContextFromSPContext(SPContext context)
    {
      BaristaContext result = new BaristaContext();

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

    public void Dispose()
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
      this.Request = null;
      this.Response = null;
    }
  }
}
