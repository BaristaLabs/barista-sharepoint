﻿namespace Barista.Library
{
    using Barista.Extensions;
    using Barista.Newtonsoft.Json;
    using Jurassic;
    using Jurassic.Library;
    using Microsoft.IdentityModel.Claims;
    using Microsoft.IdentityModel.WindowsTokenService;
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Cache;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Caching;
    using System.Xml;
    using System.Xml.Linq;
    using HttpUtility = Barista.Helpers.HttpUtility;

    [Serializable]
    public abstract class WebInstanceBase : ObjectInstance
    {
        private HttpRequestInstance m_httpRequest;
        private HttpResponseInstance m_httpResponse;

        protected WebInstanceBase(ScriptEngine engine)
            : base(engine)
        {
        }

        /// <summary>
        /// Gets or sets the HttpRequestInstance associated with the web instance and the current context.
        /// </summary>
        [JSProperty(Name = "request")]
        [JSDoc("Gets or sets the HttpRequestInstance associated with the web instance and the current context.")]
        public virtual HttpRequestInstance Request
        {
            get
            {
                if (m_httpRequest == null || (m_httpRequest != null && Equals(m_httpRequest.Request, BaristaContext.Current.Request) == false))
                {
                    m_httpRequest = new HttpRequestInstance(Engine, BaristaContext.Current.Request);
                }

                return m_httpRequest;
            }
            set { m_httpRequest = value; }
        }

        /// <summary>
        /// Gets or sets the HttpRequestInstance associated with the web instance and the current context.
        /// </summary>
        [JSProperty(Name = "response")]
        [JSDoc("Gets or sets the HttpRequestInstance associated with the web instance and the current context.")]
        public virtual HttpResponseInstance Response
        {
            get
            {
                if (m_httpResponse == null || (m_httpResponse != null && Equals(m_httpResponse.Response, BaristaContext.Current.Response) == false))
                {
                    m_httpResponse = new HttpResponseInstance(Engine, BaristaContext.Current.Response);
                }

                return m_httpResponse;
            }
            set { m_httpResponse = value; }
        }

        #region Ajax
        [JSFunction(Name = "ajax")]
        public object Ajax(string url, object settings)
        {
            //If we're running under Claims authentication, impersonate the thread user
            //by calling the Claims to Windows Token Service and call the remote site using
            //the impersonated credentials. NOTE: The Claims to Windows Token Service must be running.
            WindowsImpersonationContext ctxt = null;
            if (Thread.CurrentPrincipal.Identity is ClaimsIdentity)
            {
                IClaimsIdentity identity = (ClaimsIdentity)System.Threading.Thread.CurrentPrincipal.Identity;
                var firstClaim = identity.Claims.FirstOrDefault(c => c.ClaimType == ClaimTypes.Upn);

                if (firstClaim == null)
                    throw new InvalidOperationException("No UPN claim found");

                var upn = firstClaim.Value;

                if (String.IsNullOrEmpty(upn))
                    throw new InvalidOperationException("A UPN claim was found, however, the value was empty.");

                var currentIdentity = S4UClient.UpnLogon(upn);
                ctxt = currentIdentity.Impersonate();
            }

            try
            {
                var noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

                var targetUri = ObtainTargetUri(url);

                var request = (HttpWebRequest)WebRequest.Create(targetUri);
                request.CachePolicy = noCachePolicy; //TODO: Make this configurable.

                //Get the proxy from the concrete instance.
                var proxyAddress = ObtainDefaultProxyAddress();

                //If the proxy address is defined, create a new proxy with the address, otherwise, use the system proxy.
                request.Proxy = proxyAddress.IsNullOrWhiteSpace() == false
                  ? new WebProxy(proxyAddress, true, null, CredentialCache.DefaultNetworkCredentials)
                  : WebRequest.GetSystemWebProxy();

                if (request.Proxy != null)
                {
                    request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                }

                var dataType = AjaxDataType.Unknown;

                //If a settings parameter is defined, coerce it and set properties on the request object.
                var ajaxSettings = JurassicHelper.Coerce<AjaxSettingsInstance>(Engine, settings);
                if (ajaxSettings != null)
                {
                    if (ajaxSettings.UseDefaultCredentials == false)
                    {
                        //if the "credentials" setting is set on the ajaxsettingsinstance, use those credentials instead.
                        if (ajaxSettings.Credentials != null && ajaxSettings.Credentials != Null.Value &&
                            ajaxSettings.Credentials != Undefined.Value && ajaxSettings.Credentials is NetworkCredentialInstance)
                            request.Credentials = (ajaxSettings.Credentials as NetworkCredentialInstance).NetworkCredential;
                        //Otherwise, use the username/password/domain if specified.
                        else if (String.IsNullOrEmpty(ajaxSettings.Username) == false ||
                                 String.IsNullOrEmpty(ajaxSettings.Password) == false ||
                                 String.IsNullOrEmpty(ajaxSettings.Domain) == false)
                            request.Credentials = new NetworkCredential(ajaxSettings.Username, ajaxSettings.Password,
                              ajaxSettings.Domain);
                    }
                    else
                    {
                        request.UseDefaultCredentials = true;
                        request.Credentials = CredentialCache.DefaultNetworkCredentials;
                    }

                    if (String.IsNullOrEmpty(ajaxSettings.Accept) == false)
                        request.Accept = ajaxSettings.Accept;

                    if (ajaxSettings.Proxy != null && ajaxSettings.Proxy != Undefined.Value && ajaxSettings.Proxy != Null.Value)
                    {
                        var proxySettings = JurassicHelper.Coerce<ProxySettingsInstance>(Engine, ajaxSettings.Proxy);

                        if (proxySettings != null)
                        {
                            if (String.IsNullOrEmpty(proxySettings.Address) == false)
                            {
                                try
                                {
                                    var proxy = new WebProxy(proxySettings.Address, true);
                                    request.Proxy = proxy;
                                }
                                catch
                                {
                                    /* do nothing */
                                }
                            }

                            if (proxySettings.UseDefaultCredentials)
                                request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                        }
                    }

                    if (String.IsNullOrEmpty(ajaxSettings.DataType) == false)
                    {
                        switch (ajaxSettings.DataType.ToLowerInvariant())
                        {
                            case "json":
                                dataType = AjaxDataType.Json;
                                break;
                            case "xml":
                                dataType = AjaxDataType.Xml;
                                break;
                            case "raw":
                                dataType = AjaxDataType.Raw;
                                break;
                            default:
                                dataType = AjaxDataType.Unknown;
                                break;
                        }
                    }

                    if (String.IsNullOrEmpty(ajaxSettings.Method) == false)
                    {
                        request.Method = ajaxSettings.Method;
                    }

                    if (String.IsNullOrEmpty(ajaxSettings.UserAgent) == false)
                    {
                        request.UserAgent = ajaxSettings.UserAgent;
                    }

                    if (String.IsNullOrEmpty(ajaxSettings.ContentType) == false)
                    {
                        request.ContentType = ajaxSettings.ContentType;
                    }

                    if (String.IsNullOrEmpty(ajaxSettings.Referer) == false)
                    {
                        request.Referer = ajaxSettings.Referer;
                    }
                }
                else
                {
                    request.Accept = "application/json";
                }

                //Get some not-serialized properties from the original webinstance
                if (settings != Null.Value && settings != Undefined.Value && settings is ObjectInstance)
                {
                    var settingsObj = settings as ObjectInstance;

                    if (settingsObj.HasProperty("headers"))
                    {
                        var headersObj = settingsObj.GetPropertyValue("headers");
                        if (headersObj != Null.Value && headersObj != Undefined.Value && headersObj is ObjectInstance)
                        {
                            var headersObjInstance = headersObj as ObjectInstance;
                            foreach (var prop in headersObjInstance.Properties)
                            {
                                try
                                {
                                    request.Headers.Set(prop.Name, TypeConverter.ToString(prop.Value));
                                }
                                catch (Exception)
                                {
                                    //Ignore it.
                                }
                            }
                        }
                    }

                    if (settingsObj.HasProperty("timeout"))
                    {
                        var timeoutObj = settingsObj.GetPropertyValue("timeout");
                        request.Timeout = TypeConverter.ToInteger(timeoutObj);
                    }

                    if (settingsObj.HasProperty("body"))
                    {
                        var bodyObj = settingsObj.GetPropertyValue("body");
                        if (bodyObj != Null.Value && bodyObj != Undefined.Value)
                        {
                            var bodyByteArray = bodyObj as Base64EncodedByteArrayInstance;
                            if (bodyByteArray != null)
                            {
                                var requestStream = request.GetRequestStream();
                                requestStream.Write(bodyByteArray.Data, 0, bodyByteArray.Data.Length);
                                requestStream.Close();
                            }
                            else
                            {
                                var data = Encoding.UTF8.GetBytes(TypeConverter.ToString(bodyObj));
                                var requestStream = request.GetRequestStream();
                                requestStream.Write(data, 0, data.Length);
                                requestStream.Close();
                            }
                        }
                    }
                }

                if (ajaxSettings != null && ajaxSettings.Async)
                {
                    var tcs = new TaskCompletionSource<object>();

                    try
                    {
                        request.BeginGetResponse(iar =>
                        {
                            HttpWebResponse response = null;
                            try
                            {
                                response = (HttpWebResponse)request.EndGetResponse(iar);
                                var resultObject = GetResultFromResponse(response, dataType);
                                tcs.SetResult(resultObject);
                            }
                            catch (Exception exc) { tcs.SetException(exc); }
                            finally { if (response != null) response.Close(); }
                        }, null);
                    }
                    catch (Exception exc) { tcs.SetException(exc); }

                    return new DeferredInstance(Engine.Object.InstancePrototype, tcs.Task);
                }

                object result;
                try
                {
                    var syncResponse = (HttpWebResponse)request.GetResponse();
                    result = GetResultFromResponse(syncResponse, dataType);
                }
                catch (WebException e)
                {
                    //The request failed -- usually a 400
                    var httpResponse = e.Response as HttpWebResponse;

                    result = httpResponse == null
                      ? null
                      : new HttpWebResponseInstance(Engine.Object.InstancePrototype, httpResponse);
                }
                catch (Exception ex)
                {
                    LogAjaxException(ex);
                    throw;
                }

                return result;
            }
            finally
            {
                if (ctxt != null)
                    ctxt.Dispose();
            }
        }
        #endregion

        [JSFunction(Name = "parseQueryString")]
        public object ParseQueryString(object query)
        {
            if ((query is string) == false)
                return Null.Value;

            var result = Engine.Object.Construct();
            var dict = HttpUtility.ParseQueryString(query as string);
            foreach (var key in dict.AllKeys)
            {
                result.SetPropertyValue(key, dict[key], false);
            }
            return result;
        }

        [JSFunction(Name = "getItemFromCache")]
        public object GetItemFromCache(string cacheKey)
        {
            var result = HttpRuntime.Cache.Get(cacheKey) as string;
            if (result == null)
                return Null.Value;

            return result;
        }

        [JSFunction(Name = "addItemToCache")]
        public void AddItemToCache(string cacheKey, object item, object absoluteExpiration, object slidingExpiration)
        {
            string stringItem;
            var dtExpiration = Cache.NoAbsoluteExpiration;
            var tsExpiration = Cache.NoSlidingExpiration;

            if (item == Null.Value || item == Undefined.Value || item == null)
                return;

            if (item is ObjectInstance)
                stringItem = JSONObject.Stringify(Engine, item, null, null);
            else
                stringItem = item.ToString();

            if (absoluteExpiration != Null.Value && absoluteExpiration != Undefined.Value && absoluteExpiration != null)
            {
                if (absoluteExpiration is DateInstance)
                    dtExpiration = DateTime.Parse((absoluteExpiration as DateInstance).ToIsoString());
                else
                    dtExpiration = DateTime.Parse(absoluteExpiration.ToString());
            }

            if (slidingExpiration != Null.Value && slidingExpiration != Undefined.Value && slidingExpiration != null)
            {
                if (slidingExpiration is DateInstance)
                    tsExpiration = TimeSpan.FromMilliseconds((slidingExpiration as DateInstance).ValueInMilliseconds);
                else
                    tsExpiration = TimeSpan.Parse(slidingExpiration.ToString());
            }

            HttpRuntime.Cache.Insert(cacheKey, stringItem, null, dtExpiration, tsExpiration);
        }

        [JSFunction(Name = "removeItemFromCache")]
        public string RemoveItemFromCache(string cacheKey)
        {
            var result = HttpRuntime.Cache.Remove(cacheKey) as string;
            return result;
        }

        [JSFunction(Name = "getItemsInCache")]
        public object GetItemsInCache()
        {
            var result = Engine.Object.Construct();
            foreach (var item in HttpRuntime.Cache.OfType<DictionaryEntry>())
            {
                var key = item.Key as string;
                if (key == null)
                    continue;

                var value = item.Value as string;

                if (value == null)
                    result.SetPropertyValue(key, Null.Value, false);
                else
                    result.SetPropertyValue(key, value, false);
            }
            return result;
        }

        #region Protected Methods

        /// <summary>
        /// For the given Url, returns the corresponding target uri.
        /// </summary>
        /// <remarks>
        /// Extensibility point to allow manipulation of the url. For instance, in the SharePoint WebInstance, to 
        /// allow site-relative urls.
        /// </remarks>
        /// <param name="url"></param>
        /// <returns></returns>
        protected virtual Uri ObtainTargetUri(string url)
        {
            return new Uri(url);
        }

        /// <summary>
        /// Returns the default proxy address to use when making Ajax calls.
        /// </summary>
        /// <remarks>
        /// Extensibility point to allow a default proxy address that is defined elsewhere. For instance, in the 
        /// SharePoint WebInstance, in the farm property bag settings.
        /// </remarks>
        /// <returns></returns>
        protected virtual string ObtainDefaultProxyAddress()
        {
            return null;
        }

        /// <summary>
        /// Logs the specified exception that occurred during an Ajax request.
        /// </summary>
        /// <remarks>
        /// Extensibility point to allow ajax failures to be logged.
        /// </remarks>
        /// <param name="ex"></param>
        protected virtual void LogAjaxException(Exception ex)
        {
            //Do Nothing.
        }

        protected object GetResultFromResponse(HttpWebResponse response, AjaxDataType dataType)
        {
            object resultObject = null;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (var stream = response.GetResponseStream())
                {
                    var resultData = stream.ToByteArray();
                    var result = Encoding.UTF8.GetString(resultData);

                    var success = false;

                    //If there is no contents, return undefined.
                    if (resultData.Length == 0)
                    {
                        resultObject = Undefined.Value;
                        success = true;
                    }

                    if (dataType == AjaxDataType.Unknown || dataType == AjaxDataType.Json)
                    {
                        //Attempt to convert the result into Json
                        if (!success)
                        {
                            try
                            {
                                resultObject = JSONObject.Parse(Engine, result, null);
                                success = true;
                            }
                            catch
                            {
                                /* Do Nothing. */
                            }
                        }
                    }

                    if (dataType == AjaxDataType.Unknown || dataType == AjaxDataType.Xml)
                    {
                        if (!success)
                        {
                            //Attempt to convert the result into Xml
                            try
                            {
                                XDocument doc;
                                using (var xmlStream = new MemoryStream(resultData))
                                {
                                    using (var xmlReader = new XmlTextReader(xmlStream))
                                    {
                                        doc = XDocument.Load(xmlReader);
                                    }
                                }
                                var jsonDocument = JsonConvert.SerializeXmlNode(doc.Root.GetXmlNode());
                                resultObject = JSONObject.Parse(Engine, jsonDocument, null);
                                success = true;
                            }
                            catch
                            {
                                /* Do Nothing. */
                            }
                        }
                    }

                    if (dataType == AjaxDataType.Unknown || dataType == AjaxDataType.Raw)
                    {
                        if (!success)
                        {
                            //If we couldn't convert as json or xml, use it as a byte array.
                            resultObject = new Base64EncodedByteArrayInstance(Engine.Object.InstancePrototype, resultData);
                        }
                    }
                }
            }
            else
            {
                resultObject = String.Format("Error attempting to retrieve {2}: {0} {1}", response.StatusCode, response.StatusDescription, response.ResponseUri);
            }

            return resultObject;
        }
        #endregion
    }
}
