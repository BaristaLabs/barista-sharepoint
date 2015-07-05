namespace Barista.Library
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Barista.Extensions;
    using Barista.Helpers;
    using Jurassic;
    using Jurassic.Library;
    using System.Text;
    using System;

    [Serializable]
    public class HttpRequestInstance : ObjectInstance
    {
        private ObjectInstance m_files;

        public HttpRequestInstance(ScriptEngine engine, BrewRequest request)
            : base(engine)
        {
            Request = request;
            PopulateFields();
            PopulateFunctions();
        }

        #region Properties

        public BrewRequest Request
        {
            get;
            private set;
        }

        [JSProperty(Name = "accept")]
        [JSDoc("ternPropertyType", "[string]")]
        public ArrayInstance Accept
        {
            get
            {
                var result = Engine.Array.Construct();
                foreach(var acceptType in Request.Headers.Accept)
                {
                    var obj = Engine.Object.Construct();
                    obj.SetPropertyValue(acceptType.Item1, (double)acceptType.Item2, false);
                    ArrayInstance.Push(result, obj);
                }

                return result;
            }
        }

        [JSProperty(Name = "contentType")]
        public string ContentType
        {
            get { return Request.Headers.ContentType; }
        }

        [JSProperty(Name = "extendedProperties")]
        public HashtableInstance ExtendedProperties
        {
            get
            {
                var ht = new Hashtable();
                foreach(var kvp in Request.ExtendedProperties)
                    ht.Add(kvp.Key, kvp.Value);

                return new HashtableInstance(Engine.Object.InstancePrototype, ht);
            }
        }

        private BaristaCookieListInstance m_cookieList;
        [JSProperty(Name = "cookies")]
        public BaristaCookieListInstance Cookies
        {
            get
            {
                if (m_cookieList == null && Request.Headers.Cookies != null)
                {
                    m_cookieList = new BaristaCookieListInstance(Engine, Request.Headers.Cookies);
                }

                return m_cookieList;
            }
        }

        [JSProperty(Name = "files", IsEnumerable = true)]
        public ObjectInstance Files
        {
            get
            {
                EnsureParsedFormData();
                if (m_files == null && m_httpFiles != null)
                {
                    m_files = Engine.Object.Construct();

                    foreach (var file in m_httpFiles)
                    {
                        var content = new Base64EncodedByteArrayInstance(Engine.Object.InstancePrototype, file.Value.ToByteArray())
                          {
                              FileName = file.Name,
                              MimeType = file.ContentType
                          };

                        m_files.SetPropertyValue(file.Key, content, false);
                    }
                }

                return m_files;
            }
        }

        [JSProperty(Name = "filenames")]
        [JSDoc("ternPropertyType", "[string]")]
        public ArrayInstance Filenames
        {
            get
            {
                EnsureParsedFormData();

                // ReSharper disable CoVariantArrayConversion
                var result = Engine.Array.Construct(m_httpFiles.Select(f => f.Key).ToArray());
                // ReSharper restore CoVariantArrayConversion

                return result;
            }
        }

        [JSProperty(Name = "headers")]
        public ObjectInstance Headers
        {
            get
            {
                var result = Engine.Object.Construct();
                foreach (var key in Request.Headers.Keys)
                {
                    var value = Request.Headers[key];
                    var enumerable = value as string[] ?? value.ToArray();

                    if (enumerable.Count() == 1)
                        result.SetPropertyValue(key, enumerable.First(), true);
                    else
// ReSharper disable once CoVariantArrayConversion
                        result.SetPropertyValue(key, Engine.Array.Construct(enumerable), true);
                }
                return result;
            }
        }

        [JSProperty(Name = "location")]
        public string Location
        {
            get { return Request.Url.PathAndQuery; }
        }

        [JSProperty(Name = "form")]
        public ObjectInstance Form
        {
            get
            {
                EnsureParsedFormData();

                var result = Engine.Object.Construct();

                if (m_form == null)
                    return result;

                foreach (var key in m_form.Keys)
                {
                    result.SetPropertyValue(key, m_form[key], false);
                }

                return result;
            }
        }

        [JSProperty(Name = "body")]
        public Base64EncodedByteArrayInstance Body
        {
            get
            {
                return new Base64EncodedByteArrayInstance(Engine.Object.InstancePrototype, Request.Body);
            }
        }

        [JSProperty(Name = "queryString")]
        public ObjectInstance QueryString
        {
            get
            {
                var result = Engine.Object.Construct();
                foreach (var key in Request.QueryString.Keys)
                {
                    result.SetPropertyValue(key, Request.QueryString[key], false);
                }
                return result;
            }
        }

        [JSProperty(Name = "restUrl")]
        public string RestUrl
        {
            get
            {
                var uri = Request.Url;
                var restStartIndex = -1;
                for (var i = 0; i < uri.Segments.Length; i++)
                {
                    var currentSegment = uri.Segments[i];
                    if (currentSegment.ToLowerInvariant() == "barista.svc/" && (i + 1 < uri.Segments.Length) &&
                        (uri.Segments[i + 1].ToLowerInvariant() == "eval/" ||
                         uri.Segments[i + 1].ToLowerInvariant() == "exec/"))
                    {
                        restStartIndex = i + 1;
                    }
                }

                if (restStartIndex == -1)
                    return null;

                return uri.Segments
                    .Skip(restStartIndex + 1)
                    .Join("");
            }
        }

        [JSProperty(Name = "referrerLocation")]
        public string ReferrerLocation
        {
            get
            {
                Uri referrerUri;
                if (Uri.TryCreate(Request.Headers.Referrer, UriKind.Absolute, out referrerUri))
                    return referrerUri.PathAndQuery;
                return String.Empty;
            }
        }

        [JSProperty(Name = "referrer")]
        public string Referrer
        {
            get
            {
                return Request.Headers.Referrer;
            }
        }

        [JSProperty(Name = "method")]
        public string Method
        {
            get { return Request.Method; }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get { return Request.Url.ToString(); }
        }

        [JSProperty(Name = "userAgent")]
        public string UserAgent
        {
            get { return Request.Headers.UserAgent; }
        }

        [JSProperty(Name = "clientInfo")]
        public ClientInfoInstance BrowserInfo
        {
            get
            {
                var br = BrowserUserAgentParser.GetDefault();
                var clientInfo = br.Parse(Request.Headers.UserAgent);
                return new ClientInfoInstance(Engine.Object.InstancePrototype, clientInfo);
            }
        }
        #endregion

        [JSFunction(Name = "getBodyObject")]
        public object GetBodyObject()
        {
            var stringBody = Encoding.UTF8.GetString(Request.Body);
            if (stringBody.IsNullOrWhiteSpace())
                return Null.Value;

            return JSONObject.Parse(Engine, stringBody, null);
        }

        [JSFunction(Name = "getBodyString")]
        public string GetBodyString()
        {
            var stringBody = Encoding.UTF8.GetString(Request.Body);
            return stringBody;
        }

        private bool m_parsedFormData;
        private IDictionary<string, string> m_form; 
        private IList<HttpFile> m_httpFiles;
        private void EnsureParsedFormData()
        {
            if (m_parsedFormData)
                return;

            IList<HttpFile> files;
            m_form = Request.ParseFormData(out files);
            m_httpFiles = files;
            m_parsedFormData = true;
        }
    }
}