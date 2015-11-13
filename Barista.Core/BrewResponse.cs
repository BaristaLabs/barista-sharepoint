namespace Barista
{
    using System.Globalization;
    using Barista.Engine;
    using Barista.Extensions;
    using Barista.Library;
    using Jurassic;
    using Jurassic.Library;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Web;
    using System.Text.RegularExpressions;
    using Barista.Newtonsoft.Json;

    [DataContract(Namespace = Constants.ServiceNamespace)]
    [Serializable]
    [KnownType(typeof(Biscotti))]
    public class BrewResponse : IExtensibleDataObject
    {
        public BrewResponse()
        {
            Cookies = new List<IBaristaCookie>(2);
            ContentLength = 0;
            ContentType = "application/json";
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            StatusCode = HttpStatusCode.OK;
            StatusDescription = "OK";
            SuppressContent = false;
            ExtendedProperties = new Dictionary<string, string>();
        }

        [DataMember]
        [JsonProperty("cookies")]
        public IList<IBaristaCookie> Cookies
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty("content")]
        public Byte[] Content
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty("contentLength")]
        public long ContentLength
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty("contentType")]
        private string m_contentType;

        [IgnoreDataMember]
        public string ContentType
        {
            get { return Headers.ContainsKey("content-type") ? Headers["content-type"] : m_contentType; }
            set { m_contentType = value; }
        }

        [DataMember]
        [JsonProperty("headers")]
        public IDictionary<string, string> Headers
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty("suppressContent")]
        public bool SuppressContent
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty("statusDescription")]
        public string StatusDescription
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty("extendedProperties")]
        public IDictionary<string, string> ExtendedProperties
        {
            get;
            set;
        }

        [NonSerialized]
        private ExtensionDataObject m_extensionData;

        public ExtensionDataObject ExtensionData
        {
            get { return m_extensionData; }
            set { m_extensionData = value; }
        }

        public void ModifyOutgoingWebResponse(OutgoingWebResponseContext webResponse)
        {
            SetHttpResponseHeaders(webResponse);

            webResponse.ContentLength = ContentLength;
            webResponse.ContentType = ContentType;

            if (ContentType != null)
            {
                webResponse.ContentType = ContentType;
            }

            if (StatusDescription != null)
            {
                //Undefined things happen when the description.length > 512
                var description = Regex.Replace(StatusDescription, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
                webResponse.StatusDescription = description.Truncate(512);
            }

            webResponse.StatusCode = StatusCode;
            webResponse.SuppressEntityBody = SuppressContent;
        }

        private void SetHttpResponseHeaders(OutgoingWebResponseContext context)
        {
            foreach (var kvp in Headers)
            {
                context.Headers.Add(kvp.Key, kvp.Value);
            }
            foreach (var cookie in Cookies)
            {
                context.Headers.Add("Set-Cookie", cookie.ToString());
            }
        }

        public void ModifyHttpResponse(HttpResponse response, bool setHeaders)
        {
            if (setHeaders)
            {
                foreach (var header in Headers.Keys)
                {
                    if (response.Headers.AllKeys.Any(k => k == header))
                        response.Headers.Set(header, Headers[header]);
                    else
                        response.Headers.Add(header, Headers[header]);
                }
            }

            foreach (var cookie in Cookies)
            {
                if (response.Cookies.AllKeys.Any(k => k == cookie.Name))
                    response.Cookies.Set(new HttpCookie(cookie.Name, cookie.ToString()));
                else
                    response.Cookies.Add(new HttpCookie(cookie.Name, cookie.ToString()));
            }

            response.ContentType = ContentType;

            if (setHeaders)
            {
                var description = Regex.Replace(StatusDescription, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
                response.StatusDescription = description.Truncate(512);
            }

            response.StatusCode = (int)StatusCode;
            response.SuppressContent = SuppressContent;
        }

        /// <summary>
        /// Sets the contents property based on the contents of the result object
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="result"></param>
        /// <param name="isRaw"></param>
        public void SetContentsFromResultObject(IScriptEngine engine, object result, bool isRaw)
        {
            //If IsRaw has been set on the response script object, convert the result value as a byte array, otherwise convert the string as a byte array.
            byte[] byteArray;
            if (isRaw)
            {
                var instance = result as Base64EncodedByteArrayInstance;
                if (instance != null)
                {
                    byteArray = instance.Data;
                }
                else if (result is StringInstance || result is string)
                {
                    var stringResult = result as string ?? String.Empty;

                    byteArray = Encoding.UTF8.GetBytes(stringResult);
                }
                else
                {
                    byteArray = StringHelper.StringToByteArray(result.ToString());
                }
            }
            else
            {
                var instance = result as Base64EncodedByteArrayInstance;
                if (instance != null)
                {
                    var arrayResult = instance;
                    byteArray = arrayResult.Data;
                }
                else if (TypeUtilities.IsString(result))
                {
                    var stringResult = TypeConverter.ToString(result) ?? String.Empty;
                    byteArray = Encoding.UTF8.GetBytes(stringResult);
                }
                else
                {
                    //Obtain the script result and stringify it -- e.g. convert it to a json object.
                    var stringResult = engine.Stringify(result, null, null);
                    if (String.IsNullOrEmpty(stringResult) || (String.IsNullOrEmpty(stringResult.Trim())))
                    {
                        byteArray = new byte[0];
                    }
                    else
                    {
                        byteArray = Encoding.UTF8.GetBytes(stringResult.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            Content = byteArray;
        }


        /// <summary>
        /// Determines the content type from the specified result.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="defaultContentType"></param>
        /// <returns></returns>
        public static string AutoDetectContentTypeFromResult(object result, string defaultContentType)
        {
            var contentType = defaultContentType;
            if (contentType.IsNullOrWhiteSpace())
            {
                contentType = "application/json";
            }

            var array = result as Base64EncodedByteArrayInstance;
            if (array != null)
            {
                var base64EncodedByteArray = array;
                contentType = base64EncodedByteArray.MimeType;
            }
            else if (TypeUtilities.IsString(result))
            {
                var stringResult = TypeConverter.ToString(result);
                if (stringResult.IsNullOrWhiteSpace() == false)
                {
                    var trimmedStringResult = stringResult.TrimStart();

                    if (trimmedStringResult.StartsWith("<html", StringComparison.InvariantCultureIgnoreCase) ||
                        trimmedStringResult.StartsWith("<!doctype html>", StringComparison.InvariantCultureIgnoreCase))
                         contentType = "text/html";
                    else if (trimmedStringResult.StartsWith("<?xml", StringComparison.InvariantCultureIgnoreCase))
                        contentType = "text/xml";
                    else 
                        contentType = "text/plain";
                }
                else
                    contentType = "text/plain"; 
            }
            else if (result is ObjectInstance)
            {
                contentType = "application/json";
            }

            return contentType;
        }
    }
}
