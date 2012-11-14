namespace Barista.SharePoint.Extensions
{
  using Microsoft.SharePoint;
  using System;

  public static class BrewRequestExtensions
  {
    public static void SetExtendedPropertiesFromCurrentSPContext(this BrewRequest request)
    {
      if (SPContext.Current != null)
      {
        if (SPContext.Current.Site != null)
          request.ExtendedProperties.Add("SPSiteId", SPContext.Current.Site.ID.ToString());

        if (SPContext.Current.Web != null)
          request.ExtendedProperties.Add("SPWebId", SPContext.Current.Web.ID.ToString());

        if (SPContext.Current.ListId != null && SPContext.Current.ListId != Guid.Empty)
          request.ExtendedProperties.Add("SPListId", SPContext.Current.ListId.ToString());

        if (String.IsNullOrEmpty(SPContext.Current.ListItemServerRelativeUrl) == false)
          request.ExtendedProperties.Add("SPListItemUrl", SPContext.Current.ListItemServerRelativeUrl);

        if (SPContext.Current.ViewContext != null && SPContext.Current.ViewContext.ViewId != Guid.Empty)
          request.ExtendedProperties.Add("SPViewId", SPContext.Current.ViewContext.ViewId.ToString());

        if (SPContext.Current.File != null && SPContext.Current.File.UniqueId != Guid.Empty)
          request.ExtendedProperties.Add("SPFileId", SPContext.Current.File.UniqueId.ToString());
      }
    }
  }
}
