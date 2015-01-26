namespace Barista.SharePoint.Library
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint.Utilities;

    [Serializable]
    public class SPUtilityInstance : ObjectInstance
    {
        public SPUtilityInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        [JSProperty(Name="accessDeniedPage")]
        public string AccessDeniedPage
        {
            get
            {
                return SPUtility.AccessDeniedPage;
            }
        }

        [JSProperty(Name="authenticatePage")]
        public string AuthenticatePage
        {
            get
            {
                return SPUtility.AuthenticatePage;
            }
        }

        [JSProperty(Name="errorPage")]
        public string ErrorPage
        {
            get
            {
                return SPUtility.ErrorPage;
            }
        }

        [JSProperty(Name="successPage")]
        public string SuccessPage
        {
            get
            {
                return SPUtility.SuccessPage;
            }
        }

        [JSProperty(Name="siteRelativeUrlPrefix")]
        public string SiteRelativeUrlPrefix
        {
            get
            {
                return SPUtility.SiteRelativeUrlPrefix;
            }
        }

        [JSProperty(Name="webRelativeUrlPrefix")]
        public string WebRelativeUrlPrefix
        {
            get
            {
                return SPUtility.WebRelativeUrlPrefix;
            }
        }

        [JSProperty(Name="developerDashboardIsEnabled")]
        public bool DeveloperDashboardIsEnabled
        {
            get
            {
                return SPUtility.DeveloperDashboardIsEnabled;
            }
        }

        [JSProperty(Name="originalServerRelativeRequestPath")]
        public string OriginalServerRelativeRequestPath
        {
            get
            {
                return SPUtility.OriginalServerRelativeRequestPath;
            }
        }

        [JSProperty(Name="originalServerRelativeRequestUrl")]
        public string OriginalServerRelativeRequestUrl
        {
            get
            {
                return SPUtility.OriginalServerRelativeRequestUrl;
            }
        }

        [JSFunction(Name = "addDefaultWikiContent")]
        public string AddDefaultWikiContent(SPListInstance list)
        {
            if (list == null)
                throw new JavaScriptException(this.Engine, "Error", "A list must be specified as the first parameter.");

            return SPUtility.AddDefaultWikiContent(list.List);
        }

        [JSFunction(Name = "concatUrls")]
        public string ConcatUrls(string firstPart, string secondPart)
        {
            return SPUtility.ConcatUrls(firstPart, secondPart);
        }

        [JSFunction(Name = "createNewDiscussion")]
        public SPListItemInstance CreateNewDiscussion(SPListInstance list, string title)
        {
            if (list == null)
                throw new JavaScriptException(this.Engine, "Error", "A list must be specified as the first parameter.");

            var result = SPUtility.CreateNewDiscussion(list.List, title);
            return result == null
                ? null
                : new SPListItemInstance(this.Engine, result);
        }

        [JSFunction(Name = "createNewDiscussionReply")]
        public SPListItemInstance CreateNewDiscussionReply(SPListItemInstance parent)
        {
            if (parent == null)
                throw new JavaScriptException(this.Engine, "Error", "A listitem must be specified as the first parameter.");

            var result = SPUtility.CreateNewDiscussionReply(parent.ListItem);
            return result == null
                ? null
                : new SPListItemInstance(this.Engine, result);
        }

        [JSFunction(Name = "createNewWikiPage")]
        public SPFileInstance CreateNewWikiPage(SPListInstance list, string url)
        {
            if (list == null)
                throw new JavaScriptException(this.Engine, "Error", "A list must be specified as the first parameter.");

            var result = SPUtility.CreateNewWikiPage(list.List, url);
            return result == null
                ? null
                : new SPFileInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "ensureSiteAdminAccess")]
        public void EnsureSiteAdminAccess(SPWebInstance web)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be specified as the first parameter.");

            SPUtility.EnsureSiteAdminAccess(web.Web);
        }

        [JSFunction(Name = "flushIISToken")]
        public void FlushIisToken()
        {
            SPUtility.FlushIISToken();
        }

        [JSFunction(Name = "formatAccountName")]
        public string FormatAccountName(string user)
        {
            return SPUtility.FormatAccountName(user);
        }

        [JSFunction(Name = "formatAccountName2")]
        public string FormatAccountName2(string provider, string user)
        {
            return SPUtility.FormatAccountName(provider, user);
        }

        [JSFunction(Name = "getAccountName")]
        public string GetAccountName(string fullName)
        {
            return SPUtility.GetAccountName(fullName);
        }

        [JSFunction(Name = "getCurrentUserEmailAddresses")]
        public string GetCurrentUserEmailAddresses()
        {
            return SPUtility.GetCurrentUserEmailAddresses();
        }

        [JSFunction(Name = "getFullNameandEmailfromLogin")]
        public ObjectInstance GetFullNameandEmailfromLogin(SPWebInstance web, string login)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be specified as the first parameter.");

            string displayName;
            string email;
            SPUtility.GetFullNameandEmailfromLogin(web.Web, login, out displayName, out email);

            var result = this.Engine.Object.Construct();
            result.SetPropertyValue("displayName", displayName, false);
            result.SetPropertyValue("email", email, false);
            return result;
        }

        [JSFunction(Name = "getFullNameFromLoginEx")]
        public ObjectInstance GetFullNameFromLoginEx(string loginName)
        {
            bool isDl;
            var fullName = SPUtility.GetFullNameFromLoginEx(loginName, out isDl);

            var result = this.Engine.Object.Construct();
            result.SetPropertyValue("fullName", fullName, false);
            result.SetPropertyValue("isDL", isDl, false);
            return result;
        }

        [JSFunction(Name = "getFullNameFromLoginEx2")]
        public ObjectInstance GetFullNameFromLoginEx2(SPSiteInstance site, string loginName)
        {
            if (site == null)
                throw new JavaScriptException(this.Engine, "Error", "A site must be specified as the first parameter.");


            bool isDl;
            var fullName = SPUtility.GetFullNameFromLoginEx(site.Site, loginName, out isDl);

            var result = this.Engine.Object.Construct();
            result.SetPropertyValue("fullName", fullName, false);
            result.SetPropertyValue("isDL", isDl, false);
            return result;
        }

        [JSFunction(Name = "getFullUrl")]
        public string GetFullUrl(SPSiteInstance site, string webUrl)
        {
            if (site == null)
                throw new JavaScriptException(this.Engine, "Error", "A site must be specified as the first parameter.");

            return SPUtility.GetFullUrl(site.Site, webUrl);
        }

        
        [JSFunction(Name = "getGenericSetupPath", Deprecated = true)]
        public string GetGenericSetupPath(string subDir)
        {
            return SPUtility.GetGenericSetupPath(subDir);
        }

        [JSFunction(Name = "getGenericSetupPath")]
        public string GetGenericSetupPath(string subDir, int desiredPathVersion)
        {
            return SPUtility.GetVersionedGenericSetupPath(subDir, desiredPathVersion);
        }

        [JSFunction(Name = "getLocalizedString")]
        public string GetLocalizedString(string source, string defaultResourceFile, double language)
        {
            return SPUtility.GetLocalizedString(source, defaultResourceFile, (uint)language);
        }

        [JSFunction(Name = "getPrincipalsInGroup")]
        public ObjectInstance GetPrincipalsInGroup(SPWebInstance web, string input, int maxCount)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be specified as the first parameter.");

            bool reachedMaxCount;
            var principals = SPUtility.GetPrincipalsInGroup(web.Web, input, maxCount, out reachedMaxCount);
            var arrPrincipals = this.Engine.Array.Construct();
            foreach (var principal in principals)
                ArrayInstance.Push(arrPrincipals, new SPPrincipalInfoInstance(this.Engine, principal));

            var result = this.Engine.Object.Construct();
            result.SetPropertyValue("principals", arrPrincipals, false);
            result.SetPropertyValue("reachedMaxCount", reachedMaxCount, false);
            return result;
        }

        [JSFunction(Name = "getServerNow")]
        public DateInstance GetServerNow(SPWebInstance web)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be specified as the first parameter.");

            var result = SPUtility.GetServerNow(web.Web);
            return JurassicHelper.ToDateInstance(this.Engine, result);
        }

        [JSFunction(Name = "getSPListFromName")]
        public SPListInstance GetSPListFromName(SPWebInstance web, object webId, string listGuid, string listUrl, string listDisplayName)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be specified as the first parameter.");

            var guidWebId = GuidInstance.ConvertFromJsObjectToGuid(webId);

            var result = SPUtility.GetSPListFromName(web.Web, guidWebId, listGuid, listUrl, listDisplayName);
            return result == null
                ? null
                : new SPListInstance(this.Engine, null, null, result);
        }

        [JSFunction(Name = "getUrlDirectory")]
        public string GetUrlDirectory(string url)
        {
            return SPUtility.GetUrlDirectory(url);
        }

        [JSFunction(Name = "getUrlFileName")]
        public string GetUrlFileName(string url)
        {
            return SPUtility.GetUrlFileName(url);
        }

        [JSFunction(Name = "isEmailServerSet")]
        public bool IsEmailServerSet(SPWebInstance web)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be specified as the first parameter.");

            return SPUtility.IsEmailServerSet(web.Web);
        }

        [JSFunction(Name = "isLoginValid")]
        public bool IsLoginValid(SPSiteInstance site, string loginName)
        {
            if (site == null)
                throw new JavaScriptException(this.Engine, "Error", "A site must be specified as the first parameter.");

            return SPUtility.IsLoginValid(site.Site, loginName);
        }

        [JSFunction(Name = "makeBrowserCacheSafeLayoutsUrl")]
        public string MakeBrowserCacheSafeLayoutsUrl(string name, bool localizable)
        {
            return SPUtility.MakeBrowserCacheSafeLayoutsUrl(name, localizable);
        }

        [JSFunction(Name = "makeBrowserCacheSafeScriptResourceUrl")]
        public string MakeBrowserCacheSafeScriptResourceUrl(string resxName)
        {
            return SPUtility.MakeBrowserCacheSafeScriptResourceUrl(resxName);
        }

        [JSFunction(Name = "mapToControl")]
        public string MapToControl(SPWebInstance web, string fileName, string progId)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be specified as the first parameter.");

            return SPUtility.MapToControl(web.Web, fileName, progId);
        }

        [JSFunction(Name = "mapToIcon")]
        public string MapToIcon(SPWebInstance web, string fileName, string progId, object size)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be specified as the first parameter.");

            if (size != Undefined.Value && size != Null.Value && size != null)
            {
                IconSize isSize;
                if (TypeConverter.ToString(size).TryParseEnum(true, out isSize))
                {
                    SPUtility.MapToIcon(web.Web, fileName, progId, isSize);
                }
            }

            return SPUtility.MapToIcon(web.Web, fileName, progId);
        }

        [JSFunction(Name = "mapToServerFileRedirect")]
        public string MapToServerFileRedirect(SPWebInstance web, string fileName, string progId)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be specified as the first parameter.");

            return SPUtility.MapToServerFileRedirect(web.Web, fileName, progId);
        }

        [JSFunction(Name = "searchWindowsPrincipals")]
        public ObjectInstance SearchWindowsPrincipals(SPWebApplicationInstance webApp, string input, string principalType, int maxCount)
        {
            if (webApp == null)
                throw new JavaScriptException(this.Engine, "Error", "A webapp must be specified as the first parameter.");

            SPPrincipalType scopes;
            principalType.TryParseEnum(true, SPPrincipalType.All, out scopes);
            
            bool reachedMaxCount;
            var principals = SPUtility.SearchWindowsPrincipals(webApp.SPWebApplication, input, scopes, maxCount, out reachedMaxCount);
            var arrPrincipals = this.Engine.Array.Construct();
            foreach (var principal in principals)
                ArrayInstance.Push(arrPrincipals, new SPPrincipalInfoInstance(this.Engine, principal));

            var result = this.Engine.Object.Construct();
            result.SetPropertyValue("principals", arrPrincipals, false);
            result.SetPropertyValue("reachedMaxCount", reachedMaxCount, false);
            return result;
        }
    }
}
