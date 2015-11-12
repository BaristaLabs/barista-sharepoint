namespace Barista.SharePoint
{
    using System.IO;
    using Barista.Extensions;
    using Barista.SharePoint.Bundles;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using System;
    using Barista.Newtonsoft.Json;

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
            Site = site;
            Web = web;
        }

        public SPBaristaContext(BrewRequest request, BrewResponse response)
            : base(request, response)
        {
            if (request.ExtendedProperties == null || !request.ExtendedProperties.ContainsKey("SPSiteId"))
                return;

            var siteId = new Guid(request.ExtendedProperties["SPSiteId"]);

            if (siteId != Guid.Empty)
            {
                SPUrlZone siteUrlZone;

                if (request.ExtendedProperties.ContainsKey("SPUrlZone") &&
                  request.ExtendedProperties["SPUrlZone"].TryParseEnum(true, SPUrlZone.Default, out siteUrlZone))

                    if (request.ExtendedProperties.ContainsKey("SPUserToken"))
                    {
                        var tokenBytes = Convert.FromBase64String(request.ExtendedProperties["SPUserToken"]);
                        var userToken = new SPUserToken(tokenBytes);
                       
                        Site = new SPSite(siteId, siteUrlZone, userToken);
                    }
                    else
                    {
                        Site = new SPSite(siteId, siteUrlZone);
                    }
            }

            if (!request.ExtendedProperties.ContainsKey("SPWebId"))
                return;

            var webId = new Guid(request.ExtendedProperties["SPWebId"]);

            if (webId != Guid.Empty)
                Web = Site.OpenWeb(webId);

            if (request.ExtendedProperties.ContainsKey("SPListId"))
            {
                var listId = new Guid(request.ExtendedProperties["SPListId"]);

                if (listId != Guid.Empty)
                    List = Web.Lists[listId];

                if (request.ExtendedProperties.ContainsKey("SPViewId"))
                {
                    var viewId = new Guid(request.ExtendedProperties["SPViewId"]);

                    if (viewId != Guid.Empty)
                        View = List.Views[viewId];
                }
            }

            if (request.ExtendedProperties.ContainsKey("SPListItemUrl"))
            {
                var url = request.ExtendedProperties["SPListItemUrl"];

                if (String.IsNullOrEmpty(url) == false)
                {
                    Uri listItemUri;

                    //In the two places this gets used, this is a site-relative URL.
                    if (Uri.TryCreate(new Uri(Web.Url.EnsureEndsWith("/")), url.EnsureStartsWith("/"), out listItemUri))
                    {
                        try
                        {
                            ListItem = Web.GetListItem(listItemUri.ToString());
                        }
                        catch (FileNotFoundException)
                        {
                            //Do Nothing..
                        }
                    }
                }
            }

            if (!request.ExtendedProperties.ContainsKey("SPFileId"))
                return;

            var fileId = new Guid(request.ExtendedProperties["SPFileId"]);

            if (fileId != Guid.Empty)
                File = Web.GetFile(fileId);
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

            if (Web != null)
            {
                Web.Close();
                Web.Dispose();
                Web = null;
            }

            if (Site != null)
            {
                Site.Close();
                Site.Dispose();
                Site = null;
            }

            List = null;
            ListItem = null;

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
                    s_currentContext = CreateContextFromSPContext(SPContext.Current);
                }
                return s_currentContext;
            }
            set { s_currentContext = value; }
        }

        /// <summary>
        /// Gets a value that indicates if a current Barista context has been created.
        /// </summary>
        public new static bool HasCurrentContext
        {
            get { return s_currentContext != null; }
        }

        /// <summary>
        /// Given DataContract serialized BrewRequest/BrewResponse objects, deserialize and return a new context.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static SPBaristaContext CreateContextFromXmlRequestResponse(string request, string response)
        {
            var brewRequest = BaristaHelper.DeserializeXml<BrewRequest>(request);
            var brewResponse = BaristaHelper.DeserializeXml<BrewResponse>(response);
            return new SPBaristaContext(brewRequest, brewResponse);
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
                result.Site = new SPSite(context.Site.ID, context.Site.Zone, context.Site.UserToken);

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
