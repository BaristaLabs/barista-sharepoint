using System.Globalization;

namespace Barista
{
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

    [DataContract(Namespace = Constants.ServiceNamespace)]
    [Serializable]
    public class BrewResponse : IExtensibleDataObject
    {
        public BrewResponse()
        {
            this.Cookies = new List<IBaristaCookie>(2);
            this.ContentLength = 0;
            this.ContentType = "application/json";
            this.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.StatusCode = HttpStatusCode.OK;
            this.StatusDescription = "OK";
            this.SuppressContent = false;
            this.ExtendedProperties = new Dictionary<string, string>();
        }

        [DataMember]
        public IList<IBaristaCookie> Cookies
        {
            get;
            set;
        }

        [DataMember]
        public Byte[] Content
        {
            get;
            set;
        }

        [DataMember]
        public long ContentLength
        {
            get;
            set;
        }

        [DataMember]
        private string m_contentType;

        [IgnoreDataMember]
        public string ContentType
        {
            get { return Headers.ContainsKey("content-type") ? Headers["content-type"] : this.m_contentType; }
            set { this.m_contentType = value; }
        }

        [DataMember]
        public IDictionary<string, string> Headers
        {
            get;
            set;
        }

        [DataMember]
        public HttpStatusCode StatusCode
        {
            get;
            set;
        }

        [DataMember]
        public bool SuppressContent
        {
            get;
            set;
        }

        [DataMember]
        public string StatusDescription
        {
            get;
            set;
        }

        [DataMember]
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

            webResponse.ContentLength = this.ContentLength;
            webResponse.ContentType = this.ContentType;

            if (ContentType != null)
            {
                webResponse.ContentType = ContentType;
            }

            if (StatusDescription != null)
            {
                webResponse.StatusDescription = StatusDescription;
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
                foreach (var header in this.Headers.Keys)
                {
                    if (response.Headers.AllKeys.Any(k => k == header))
                        response.Headers.Set(header, this.Headers[header]);
                    else
                        response.Headers.Add(header, this.Headers[header]);
                }
            }

            foreach (var cookie in this.Cookies)
            {
                if (response.Cookies.AllKeys.Any(k => k == cookie.Name))
                    response.Cookies.Set(new HttpCookie(cookie.Name, cookie.ToString()));
                else
                    response.Cookies.Add(new HttpCookie(cookie.Name, cookie.ToString()));
            }

            response.ContentType = this.ContentType;

            if (setHeaders)
                response.StatusDescription = this.StatusDescription;

            response.StatusCode = (int)this.StatusCode;
            response.SuppressContent = this.SuppressContent;
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
                if (result is Base64EncodedByteArrayInstance)
                {
                    byteArray = (result as Base64EncodedByteArrayInstance).Data;
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
            else if (result is Base64EncodedByteArrayInstance)
            {
                var arrayResult = result as Base64EncodedByteArrayInstance;
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

            this.Content = byteArray;
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

            if (result is Base64EncodedByteArrayInstance)
            {
                var base64EncodedByteArray = result as Base64EncodedByteArrayInstance;
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
