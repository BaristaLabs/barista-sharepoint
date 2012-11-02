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
    public static BaristaContext s_currentContext = null;

    public static BaristaContext Current
    {
      get { return s_currentContext; }
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
