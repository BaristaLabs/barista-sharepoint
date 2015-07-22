namespace Barista.SharePoint
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    using Microsoft.SharePoint;
    using System.IO;
    using System.Web;
    using Microsoft.SharePoint.Utilities;
    using System.Collections;
    using Jurassic.Library;
    using Jurassic;

    /// <summary>
    /// Contains various helper methods for interacting with SharePoint.
    /// </summary>
    public static class SPHelper
    {
        private const uint ProcessToken = 0x0008;

        /// <summary>
        /// Returns a value that indicates if the process identity matches the current identity (and thus is elevated)
        /// </summary>
        /// <returns></returns>
        public static bool IsElevated()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                return identity != null && GetProcessIdentity().User == identity.User;
            }
        }

        private static WindowsIdentity GetProcessIdentity()
        {
            var handle = IntPtr.Zero;
            try
            {
                var process = Process.GetCurrentProcess();
                OpenProcessToken(process.Handle, ProcessToken, out handle);
                return new WindowsIdentity(handle);
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    CloseHandle(handle);
                }
            }
        }

        [DllImport("kernel32.dll")]
        public static extern Boolean CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr processHandle, UInt32 desiredAccess, out IntPtr tokenHandle);

        public static bool TryGetSPUserFromLoginName(string loginName, out SPUser user)
        {
            if (String.IsNullOrEmpty(loginName) || loginName == Undefined.Value.ToString())
            {
                user = SPBaristaContext.Current.Web.CurrentUser;
            }
            else
            {
                try
                {
                    user = SPBaristaContext.Current.Web.AllUsers[loginName] ??
                           SPBaristaContext.Current.Web.EnsureUser(loginName);
                }
                catch (SPException)
                {
                    user = SPBaristaContext.Current.Web.EnsureUser(loginName);
                }
            }

            return user != null;
        }

        /// <summary>
        /// Attempts to get the group with the specified name from the current site.
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static bool TryGetSPGroupFromGroupName(string groupName, out SPGroup group)
        {
            group = SPBaristaContext.Current.Web.SiteGroups[groupName];

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

            Uri finalUri;
            if (Uri.TryCreate(uriString, UriKind.Absolute, out finalUri))
            {
                uri = finalUri;
                return true;
            }

            if (SPBaristaContext.HasCurrentContext && Uri.TryCreate(SPUtility.ConcatUrls(SPBaristaContext.Current.Web.Url, uriString), UriKind.Absolute, out finalUri))
            {
                uri = finalUri;
                return true;
            }


            if (SPContext.Current != null && Uri.TryCreate(SPUtility.ConcatUrls(SPContext.Current.Web.Url, uriString), UriKind.Absolute, out finalUri))
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
            site = null;

            Uri siteUri;
            if (TryCreateWebAbsoluteUri(siteUrl, out siteUri) == false)
                return false;

            try
            {
                using (var innerSite = new SPSite(siteUri.ToString(), SPBaristaContext.Current.Site.UserToken))
                {
                    site = innerSite;
                    return true;
                }
            }
            catch
            {
                if (site != null)
                    site.Dispose();
            }

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
            web = null;
            Uri webUri;
            if (TryCreateWebAbsoluteUri(webUrl, out webUri) == false)
                return false;

            try
            {
                using (var site = new SPSite(webUri.ToString(), SPBaristaContext.Current.Site.UserToken))
                {
                    using (var innerWeb = site.OpenWeb())
                    {
                        web = innerWeb;
                        return web.Exists;
                    }
                }
            }
            catch
            {
                if (web != null)
                    web.Dispose();
            }

            web = null;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve a file at the specified url.
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
                using (var site = new SPSite(fileUri.ToString(), SPBaristaContext.Current.Site.UserToken))
                {
                    using (var web = site.OpenWeb())
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
        /// <param name="eTag"></param>
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
                using (var site = new SPSite(fileUri.ToString(), SPBaristaContext.Current.Site.UserToken))
                {
                    using (var web = site.OpenWeb())
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
        /// <param name="web"></param>
        /// <param name="folder"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public static bool TryGetSPFolder(string folderUrl, out SPSite site, out SPWeb web, out SPFolder folder)
        {
            site = null;
            web = null;
            folder = null;

            Uri folderUri;
            if (TryCreateWebAbsoluteUri(folderUrl, out folderUri) == false)
                return false;

            try
            {
                site = new SPSite(folderUri.ToString(), SPBaristaContext.Current.Site.UserToken);
                web = site.OpenWeb();
                folder = web.GetFolder(folderUri.ToString());
                if (folder != null && folder.Exists)
                    return true;

                web.Dispose();
                site.Dispose();
                return false;
            }
            catch
            {
                //Clean up.
                if (site != null)
                    site.Dispose();

                if (web != null)
                    web.Dispose();
            }

            site = null;
            web = null;
            folder = null;
            return false;
        }


        /// <summary>
        /// Attempts to retrieve the list at the specified url
        /// </summary>
        /// <param name="listUrl"></param>
        /// <param name="web"></param>
        /// <param name="list"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public static bool TryGetSPList(string listUrl, out SPSite site, out SPWeb web, out SPList list)
        {
            site = null;
            web = null;
            list = null;

            Uri listUri;
            if (TryCreateWebAbsoluteUri(listUrl, out listUri) == false)
                return false;

            try
            {
                site = new SPSite(listUri.ToString(), SPBaristaContext.Current.Site.UserToken);
                web = site.OpenWeb();
                list = web.GetList(listUri.ToString());
                return true;
            }
            catch
            {
                if (site != null)
                    site.Dispose();

                if (web != null)
                    web.Dispose();
            }

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
                using (var site = new SPSite(listUri.ToString(), SPBaristaContext.Current.Site.UserToken))
                {
                    using (var web = site.OpenWeb())
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
                using (var site = new SPSite(listItemUri.ToString(), SPBaristaContext.Current.Site.UserToken))
                {
                    using (var web = site.OpenWeb())
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
                var rawValue = listItem[fieldId];

                if (rawValue == null)
                    return false;

                value = (T)rawValue;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to get a field value of the specified type from the specified list item version and field id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="version"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetSPFieldValue<T>(this SPListItemVersion version, int index, out T value)
        {
            value = default(T);

            try
            {
                var rawValue = version[index];

                if (rawValue == null)
                    return false;

                value = (T)rawValue;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to get a field value of the specified type from the specified list item version and field id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="version"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetSPFieldValue<T>(this SPListItemVersion version, string fieldName, out T value)
        {
            value = default(T);

            try
            {
                var rawValue = version[fieldName];

                if (rawValue == null)
                    return false;

                value = (T)rawValue;
                return true;
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
        /// <param name="filePath"></param>
        /// <param name="fileContents"></param>
        /// <param name="isHiveFile"></param>
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

            //Attempt to retrieve the file from the specified SPSite.
            try
            {
                using (var sourceSite = new SPSite(fileUri.ToString(), SPBaristaContext.Current.Site.UserToken))
                {
                    using (var sourceWeb = sourceSite.OpenWeb())
                    {
                        if (sourceWeb == null)
                            throw new InvalidOperationException("The specified script url is invalid.");

                        filePath = fileUri.ToString();
                        var fileOrFolder = sourceWeb.GetFileOrFolderObject(fileUri.ToString());
                        var file = fileOrFolder as SPFile;
                        if (file != null && file.Exists)
                        {
                            Encoding encoding;
                            var content = file.OpenBinary();
                            if (content[0] == '\x00EF' && content[1] == '\x00BB' && content[2] == '\x00BF')
                                encoding = Encoding.UTF8;
                            else if (content[0] == '\x00FF' && content[1] == '\x00FE')
                                encoding = Encoding.Unicode;
                            else if (content[0] == '\x00FE' && content[1] == '\x00FF')
                                encoding = Encoding.BigEndianUnicode;
                            else
                                encoding = Encoding.ASCII;

                            fileContents = encoding.GetString(content);

                            //Yank out preamble, if it exists.
                            var byteOrderMark = encoding.GetString(encoding.GetPreamble());
                            if (fileContents.StartsWith(fileContents))
                                fileContents = fileContents.Remove(0, byteOrderMark.Length);

                            return true;
                        }
                    }
                }
            }
            catch { /* Do Nothing... */ }

            //Attempt to get the script from a relative path to the requesting url.
            Uri referrer = null;

            if (SPBaristaContext.HasCurrentContext && SPBaristaContext.Current.Request != null && SPBaristaContext.Current.Request.Headers.Referrer != null)
                referrer = new Uri(SPBaristaContext.Current.Request.Headers.Referrer, UriKind.Absolute);
            else if (HttpContext.Current != null)
                referrer = HttpContext.Current.Request.UrlReferrer;

            //Attempt to get the file relative to the url referrer.
            if (referrer != null)
            {
                try
                {
                    var url = SPUtility.ConcatUrls(SPUtility.GetUrlDirectory(referrer.ToString()), fileUrl);

                    using (var sourceSite = new SPSite(url, SPBaristaContext.Current.Site.UserToken))
                    {
                        using (var sourceWeb = sourceSite.OpenWeb())
                        {
                            if (sourceWeb == null)
                                throw new InvalidOperationException("The specified script url is invalid.");

                            filePath = url.ToString(CultureInfo.InvariantCulture);

                            var fileOrFolder = sourceWeb.GetFileOrFolderObject(fileUri.ToString());
                            var file = fileOrFolder as SPFile;

                            if (file != null && file.Exists)
                            {
                                var content = file.OpenBinary();
                                fileContents = System.Text.Encoding.UTF8.GetString(content);
                                return true;
                            }
                        }
                    }
                }
                catch { /* Do Nothing... */ }
            }

            //Attempt to get the file relative to the sharepoint hive.
            try
            {
                var hiveFileContents = String.Empty;
                var hiveFileResult = false;
                if (HttpContext.Current != null)
                {
                    var path = HttpContext.Current.Request.MapPath(fileUrl);
                    if (File.Exists(path))
                    {
                        hiveFileContents = File.ReadAllText(path);
                        hiveFileResult = true;
                    }


                    if (hiveFileResult)
                    {
                        filePath = path;
                        fileContents = hiveFileContents;
                        isHiveFile = true;
                        return true;
                    }
                }
            }
            catch { /* Do Nothing... */ }

            filePath = null;
            fileContents = null;
            return false;
        }

        public static bool TryGetHiveFileInfo(string fileUrl, out FileInfo hiveFileInfo)
        {
            hiveFileInfo = null;

            //Attempt to get the file relative to the sharepoint hive.
            try
            {
                var path = HttpRuntime.AppDomainAppPath + fileUrl.TrimStart('/').Replace("~", string.Empty).Replace('/', '\\');
                if (File.Exists(path))
                {
                    hiveFileInfo = new FileInfo(path);
                    return true;
                }
            }
            catch { /* Do Nothing... */ }

            return false;
        }

        /// <summary>
        /// Replaces SharePoint (and custom) tokens in the specified string with their corresponding values.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ReplaceTokens(string text)
        {
            if (SPBaristaContext.Current == null)
                return text;

            var result = text;
            if (SPBaristaContext.Current.List != null)
                result = result.Replace("{ListUrl}", SPUtility.ConcatUrls(SPBaristaContext.Current.Web.Url, SPBaristaContext.Current.List.RootFolder.Url));

            if (SPBaristaContext.Current.Web != null)
            {
                result = result.Replace("{WebUrl}", SPBaristaContext.Current.Web.Url);
                result = result.Replace("~site", SPBaristaContext.Current.Web.ServerRelativeUrl);
            }

            if (SPBaristaContext.Current.Site != null)
            {
                result = result.Replace("{SiteUrl}", SPBaristaContext.Current.Site.Url);
                result = result.Replace("~sitecollection", SPBaristaContext.Current.Site.ServerRelativeUrl);
            }

            return result;
        }

        /// <summary>
        /// Replaces SharePoint (and custom) tokens in the specified string with their corresponding values.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ReplaceTokens(SPContext context, string text)
        {
            if (context == null)
                return text;

            var result = text;
            if (context.List != null)
                result = result.Replace("{ListUrl}", SPUtility.ConcatUrls(context.Web.Url, context.List.RootFolder.Url));

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
                    var propertyName = property.Name.ToString(CultureInfo.InvariantCulture);

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
                                propertyValue = new SPFieldUserValue(SPBaristaContext.Current.Web, fieldValue.GetPropertyValue("fieldValue").ToString());
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
