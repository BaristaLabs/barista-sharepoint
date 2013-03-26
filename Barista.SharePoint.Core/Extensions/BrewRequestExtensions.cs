namespace Barista.SharePoint.Extensions
{
  using Microsoft.SharePoint;
  using Newtonsoft.Json;
  using System;

  public static class BrewRequestExtensions
  {
    public static void SetExtendedPropertiesFromCurrentSPContext(this BrewRequest request)
    {
      if (request == null)
        return;

      if (SPContext.Current == null)
        return;

      if (SPContext.Current.Site != null)
        request.ExtendedProperties.Add("SPSiteId", SPContext.Current.Site.ID.ToString());

      if (SPContext.Current.Web != null)
        request.ExtendedProperties.Add("SPWebId", SPContext.Current.Web.ID.ToString());

      if (SPContext.Current.ListId != default(Guid) && SPContext.Current.ListId != Guid.Empty)
        request.ExtendedProperties.Add("SPListId", SPContext.Current.ListId.ToString());

      if (String.IsNullOrEmpty(SPContext.Current.ListItemServerRelativeUrl) == false)
        request.ExtendedProperties.Add("SPListItemUrl", SPContext.Current.ListItemServerRelativeUrl);

      if (SPContext.Current.ViewContext != null && SPContext.Current.ViewContext.ViewId != Guid.Empty)
        request.ExtendedProperties.Add("SPViewId", SPContext.Current.ViewContext.ViewId.ToString());

      if (SPContext.Current.File != null && SPContext.Current.File.UniqueId != Guid.Empty)
        request.ExtendedProperties.Add("SPFileId", SPContext.Current.File.UniqueId.ToString());
    }

    public static void SetExtendedPropertiesFromSPItemEventProperties(this BrewRequest request, SPWeb web, SPList list, SPListItem item, SPItemEventProperties properties)
    {
      if (request == null)
        return;

      if (properties == null)
        return;

      var baristaProperties = BaristaItemEventProperties.CreateItemEventProperties(properties);
      var value = JsonConvert.SerializeObject(baristaProperties);

      if (properties.SiteId != default(Guid))
        request.ExtendedProperties.Add("SPSiteId", properties.SiteId.ToString());

      if (web != null)
        request.ExtendedProperties.Add("SPWebId", web.ID.ToString());

      if (list != null)
        request.ExtendedProperties.Add("SPListId", list.ID.ToString());

      if (item != null)
        request.ExtendedProperties.Add("SPListItemUrl", item.Url);

      request.ExtendedProperties.Add("SPItemEventProperties", value);
    }
  }
}
