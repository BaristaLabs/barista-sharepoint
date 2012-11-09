using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Barista.SharePoint
{
  public sealed class BaristaContext : IDisposable
  {
    private static object s_syncRoot = new object();
    private static BaristaContext s_currentContext = null;

    public static BaristaContext Current
    {
      get
      {
        if (s_currentContext == null && SPContext.Current != null)
        {
          lock (s_syncRoot)
          {
            if (s_currentContext == null && SPContext.Current != null)
            {
              s_currentContext = BaristaContext.CreateContextFromSPContext(SPContext.Current);
            }
          }
        }
        return s_currentContext;
      }
      internal set { s_currentContext = value; }
    }

    public BaristaContext()
    {
    }

    public SPFile File
    {
      get;
      internal set;
    }
    public SPSite Site
    {
      get;
      internal set;
    }

    public SPWeb Web
    {
      get;
      internal set;
    }

    public SPList List
    {
      get;
      internal set;
    }

    public SPListItem ListItem
    {
      get;
      internal set;
    }

    public SPView View
    {
      get;
      internal set;
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
        this.Web.Dispose();
        this.Web = null;
      }

      if (this.Site != null)
      {
        this.Site.Dispose();
        this.Site = null;
      }
    }
  }
}
