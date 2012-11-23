using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.IO;
using System.Web;
using Microsoft.SharePoint.Utilities;
using System.Collections;
using Jurassic.Library;
using Jurassic;

namespace Barista.SharePoint
{
  /// <summary>
  /// Contains various helper methods for interacting with SharePoint.
  /// </summary>
  public static class SPHelper
  {
    public static bool TryGetSPUserFromLoginName(string loginName, out SPUser user)
    {
      user = null;
      if (String.IsNullOrEmpty(loginName) || loginName == Undefined.Value.ToString())
      {
        user = BaristaContext.Current.Web.CurrentUser;
      }
      else
      {
        user = BaristaContext.Current.Web.AllUsers[loginName];

        if (user == null)
        {
          user = BaristaContext.Current.Web.EnsureUser(loginName);
        }
      }

      if (user == null)
        return false;

      return true;
    }

    /// <summary>
    /// Attempts to get the group with the specified name from the current site.
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public static bool TryGetSPGroupFromGroupName(string groupName, out SPGroup group)
    {
      group = BaristaContext.Current.Web.SiteGroups[groupName];

      if (group == null)
        return false;

      return true;
    }

    /// <summary>
    /// Attemps to create an absolute Uri based on the specified string. If the passed string is relative, uses the current web's uri as the base uri.
    /// </summary>
    /// <param name="uriString"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static bool TryCreateWebAbsoluteUri(string uriString, out Uri uri)
    {
      uri = null;

      if (Uri.IsWellFormedUriString(uriString, UriKind.RelativeOrAbsolute) == false)
      {
        return false;
      }

      Uri finalUri = null;
      if (Uri.TryCreate(uriString, UriKind.Absolute, out finalUri))
      {
        uri = finalUri;
        return true;
      }

      if (BaristaContext.HasCurrentContext && Uri.TryCreate(new Uri(BaristaContext.Current.Web.Url), uriString, out finalUri))
      {
        uri = finalUri;
        return true;
      }

      if (SPContext.Current != null && Uri.TryCreate(new Uri(SPContext.Current.Web.Url), uriString, out finalUri))
      {
        uri = finalUri;
        return true;
      }

      return false;
    }

    /// <summary>
    /// Attempts to retrieve a site at the specified url.
    /// </summary>
    /// <param name="siteUrl"></param>
    /// <param name="site"></param>
    /// <returns></returns>
    public static bool TryGetSPSite(string siteUrl, out SPSite site)
    {
      Uri siteUri;
      if (TryCreateWebAbsoluteUri(siteUrl, out siteUri) == false)
      {
        site = null;
        return false;
      }

      try
      {
        using (SPSite innerSite = new SPSite(siteUri.ToString()))
        {
          site = innerSite;
          return true;
        }
      }
      catch { /* Do Nothing... */ }

      site = null;
      return false;
    }

    /// <summary>
    /// Attemps to retrieve a web at the specified url.
    /// </summary>
    /// <param name="webUrl"></param>
    /// <param name="web"></param>
    /// <returns></returns>
    public static bool TryGetSPWeb(string webUrl, out SPWeb web)
    {
      Uri webUri;
      if (TryCreateWebAbsoluteUri(webUrl, out webUri) == false)
      {
        web = null;
        return false;
      }

      try
      {
        using (SPSite site = new SPSite(webUri.ToString()))
        {
          using (SPWeb innerWeb = site.OpenWeb())
          {
            web = innerWeb;
            return web.Exists;
          }
        }
      }
      catch { /* Do Nothing... */ }

      web = null;
      return false;
    }

    /// <summary>
    /// Attemps to retrieve a file at the specified url.
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool TryGetSPFile(string fileUrl, out SPFile file)
    {
      Uri fileUri;
      if (TryCreateWebAbsoluteUri(fileUrl, out fileUri) == false)
      {
        file = null;
        return false;
      }

      try
      {
        using (SPSite site = new SPSite(fileUri.ToString()))
        {
          using (SPWeb web = site.OpenWeb())
          {
            file = web.GetFile(fileUri.ToString());
            return file.Exists;
          }
        }
      }
      catch { /* Do Nothing... */ }

      file = null;
      return false;
    }

    /// <summary>
    /// Attemps to retrieve a file at the specified url.
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool TryGetSPFileETag(string fileUrl, out string eTag)
    {
      eTag = null;

      Uri fileUri;
      if (TryCreateWebAbsoluteUri(fileUrl, out fileUri) == false)
      {
        return false;
      }

      try
      {
        using (SPSite site = new SPSite(fileUri.ToString()))
        {
          using (SPWeb web = site.OpenWeb())
          {
            var file = web.GetFile(fileUri.ToString());
            if (file.Exists)
              eTag = file.ETag;
            return file.Exists;
          }
        }
      }
      catch { /* Do Nothing... */ }

      return false;
    }

    /// <summary>
    /// Attempts to retrieve a folder at the specified url.
    /// </summary>
    /// <param name="folderUrl"></param>
    /// <param name="folder"></param>
    /// <returns></returns>
    public static bool TryGetSPFolder(string folderUrl, out SPSite site, out SPWeb web, out SPFolder folder)
    {
      Uri folderUri;
      if (TryCreateWebAbsoluteUri(folderUrl, out folderUri) == false)
      {
        site = null;
        web = null;
        folder = null;
        return false;
      }

      try
      {
        site = new SPSite(folderUri.ToString());
        web = site.OpenWeb();
        folder = web.GetFolder(folderUri.ToString());
        return true;
      }
      catch { /* Do Nothing... */ }

      site = null;
      web = null;
      folder = null;
      return false;
    }


    /// <summary>
    /// Attempts to retrieve the list at the specified url
    /// </summary>
    /// <param name="listUrl"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static bool TryGetSPList(string listUrl, out SPSite site, out SPWeb web, out SPList list)
    {
      Uri listUri;
      if (TryCreateWebAbsoluteUri(listUrl, out listUri) == false)
      {
        site = null;
        web = null;
        list = null;
        return false;
      }

      try
      {
        site = new SPSite(listUri.ToString());
        web = site.OpenWeb();
        list = web.GetList(listUri.ToString());
        return true;
      }
      catch { /* Do Nothing... */ }

      site = null;
      web = null;
      list = null;
      return false;
    }

    /// <summary>
    /// Attemps to return the list view at the specified url.
    /// </summary>
    /// <param name="listUrl"></param>
    /// <param name="view"></param>
    /// <returns></returns>
    public static bool TryGetSPView(string listUrl, out SPView view)
    {
      Uri listUri;
      if (TryCreateWebAbsoluteUri(listUrl, out listUri) == false)
      {
        view = null;
        return false;
      }

      try
      {
        using (SPSite site = new SPSite(listUri.ToString()))
        {
          using (SPWeb web = site.OpenWeb())
          {
            view = web.GetViewFromUrl(listUri.ToString());
            return true;
          }
        }
      }
      catch { /* Do Nothing... */ }

      view = null;
      return false;
    }

    /// <summary>
    /// Attemps to retrieve the list item at the specified list item url.
    /// </summary>
    /// <param name="listItemUrl"></param>
    /// <param name="listItem"></param>
    /// <returns></returns>
    public static bool TryGetSPListItem(string listItemUrl, out SPListItem listItem)
    {
      Uri listItemUri;
      if (TryCreateWebAbsoluteUri(listItemUrl, out listItemUri) == false)
      {
        listItem = null;
        return false;
      }

      try
      {
        using (SPSite site = new SPSite(listItemUri.ToString()))
        {
          using (SPWeb web = site.OpenWeb())
          {
            listItem = web.GetListItem(listItemUri.ToString());
            return true;
          }
        }
      }
      catch { /* Do Nothing... */ }

      listItem = null;
      return false;
    }

    /// <summary>
    /// Attempts to get a field value of the specified type from the specified list item and field id.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="listItem"></param>
    /// <param name="fieldId"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetSPFieldValue<T>(this SPListItem listItem, Guid fieldId, out T value)
    {
      value = default(T);

      try
      {
        object rawValue = listItem[fieldId];

        if (rawValue != null)
        {
          value = (T)rawValue;
          return true;
        }
        else
        {
          return false;
        }
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Attempts to get the contents of the specified file at the specified url. Attempts to be smart about finding the location of the file.
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <param name="fileContents"></param>
    /// <returns></returns>
    public static bool TryGetSPFileAsString(string fileUrl, out string filePath, out string fileContents, out bool isHiveFile)
    {
      isHiveFile = false;

      Uri fileUri;
      if (TryCreateWebAbsoluteUri(fileUrl, out fileUri) == false)
      {
        filePath = null;
        fileContents = null;
        return false;
      }
      
      try
      {
        using (SPSite sourceSite = new SPSite(fileUri.ToString()))
        {
          using (SPWeb sourceWeb = sourceSite.OpenWeb())
          {
            if (sourceWeb == null)
              throw new InvalidOperationException("The specified script url is invalid.");

            filePath = fileUri.ToString();
            fileContents = sourceWeb.GetFileAsString(fileUri.ToString());
            return true;
          }
        }
      }
      catch { /* Do Nothing... */ }

      Uri referrer = null;

      if (BaristaContext.HasCurrentContext && BaristaContext.Current.Request != null && BaristaContext.Current.Request.UrlReferrer != null)
        referrer = BaristaContext.Current.Request.UrlReferrer;
      else if (HttpContext.Current != null && HttpContext.Current.Request != null)
        referrer = HttpContext.Current.Request.UrlReferrer;

      //Attempt to get the file relative to the url referrer.
      if (referrer != null)
      {
        try
        {
          string url = SPUtility.ConcatUrls(SPUtility.GetUrlDirectory(referrer.ToString()), fileUrl);

          using (SPSite sourceSite = new SPSite(url))
          {
            using (SPWeb sourceWeb = sourceSite.OpenWeb())
            {
              if (sourceWeb == null)
                throw new InvalidOperationException("The specified script url is invalid.");

              filePath = url.ToString();
              fileContents = sourceWeb.GetFileAsString(url.ToString());
              return true;
            }
          }
        }
        catch { /* Do Nothing... */ }
      }

      //Attempt to get the file relative to the sharepoint hive.
      try
      {
        string hiveFileContents = String.Empty;
        bool hiveFileResult = false;
        string path = String.Empty;
        SPSecurity.RunWithElevatedPrivileges(() =>
        {
          path = HttpContext.Current.Request.MapPath(fileUrl);
          if (File.Exists(path))
          {
            hiveFileContents = File.ReadAllText(path);
            hiveFileResult = true;
          }
        });

        if (hiveFileResult == true)
        {
          filePath = path;
          fileContents = hiveFileContents;
          isHiveFile = true;
          return true;
        }
      }
      catch { /* Do Nothing... */ }

      filePath = null;
      fileContents = null;
      return false;
    }

    /// <summary>
    /// Replaces SharePoint (and custom) tokens in the specified string with their corresponding values.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string ReplaceTokens(string text)
    {
      if (BaristaContext.Current == null)
        return text;

      string result = text;
      if (BaristaContext.Current.List != null)
        result = result.Replace("{ListUrl}", BaristaContext.Current.Web.Url + "/" + BaristaContext.Current.List.RootFolder.Url);

      if (BaristaContext.Current.Web != null)
      {
        result = result.Replace("{WebUrl}", BaristaContext.Current.Web.Url);
        result = result.Replace("~site", BaristaContext.Current.Web.ServerRelativeUrl);
      }

      if (BaristaContext.Current.Site != null)
      {
        result = result.Replace("{SiteUrl}", BaristaContext.Current.Site.Url);
        result = result.Replace("~sitecollection", BaristaContext.Current.Site.ServerRelativeUrl);
      }

      return result;
    }

    /// <summary>
    /// Replaces SharePoint (and custom) tokens in the specified string with their corresponding values.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string ReplaceTokens(SPContext context, string text)
    {
      if (context == null)
        return text;

      string result = text;
      if (context.List != null)
        result = result.Replace("{ListUrl}", context.Web.Url + "/" + context.List.RootFolder.Url);

      if (context.Web != null)
      {
        result = result.Replace("{WebUrl}", context.Web.Url);
        result = result.Replace("~site", context.Web.ServerRelativeUrl);
      }

      if (context.Site != null)
      {
        result = result.Replace("{SiteUrl}", context.Site.Url);
        result = result.Replace("~sitecollection", context.Site.ServerRelativeUrl);
      }

      return result;
    }

    /// <summary>
    /// For the specified object, attempt to create a SharePoint Field Values Hashtable based on the object's contents.
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static Hashtable GetFieldValuesHashtableFromPropertyObject(object properties)
    {
      Hashtable htProperties = null;
      if (properties != null && properties != Null.Value && properties is ObjectInstance)
      {
        var propertiesInstance = properties as ObjectInstance;
        htProperties = new Hashtable();
        foreach (var property in propertiesInstance.Properties)
        {
          var propertyName = property.Name.ToString();

          //TODO: Convert to appropriate FieldValues
          object propertyValue;
          if (property.Value is ObjectInstance && (property.Value as ObjectInstance).HasProperty("type"))
          {
            var fieldValue = (property.Value as ObjectInstance);

            var type = fieldValue.GetPropertyValue("type");
            switch (type.ToString())
            {
              case "Lookup":
                propertyValue = new SPFieldLookupValue(fieldValue.GetPropertyValue("fieldValue").ToString());
                break;
              case "MultiChoice":
                propertyValue = new SPFieldMultiChoiceValue(fieldValue.GetPropertyValue("fieldValue").ToString());
                break;
              case "MultiChoiceColumn":
                propertyValue = new SPFieldMultiColumnValue(fieldValue.GetPropertyValue("fieldValue").ToString());
                break;
              case "Url":
                propertyValue = new SPFieldUrlValue(fieldValue.GetPropertyValue("fieldValue").ToString());
                break;
              case "User":
                //TODO: Maybe get another web?
                propertyValue = new SPFieldUserValue(BaristaContext.Current.Web, fieldValue.GetPropertyValue("fieldValue").ToString());
                break;
              default:
                propertyValue = fieldValue.ToString();
                break;
            }
          }
          else
          {
            propertyValue = property.Value.ToString();
          }

          htProperties.Add(propertyName, propertyValue);
        }
      }

      return htProperties;
    }
  }
}
