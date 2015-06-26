namespace Barista.Library
{
    using System.Collections;
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
            // ReSharper disable CoVariantArrayConversion
            get { return Engine.Array.Construct(Request.AcceptTypes); }
            // ReSharper restore CoVariantArrayConversion
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

        [JSProperty(Name = "contentType")]
        public string ContentType
        {
            get { return Request.ContentType; }
        }

        [JSProperty(Name = "files", IsEnumerable = true)]
        public ObjectInstance Files
        {
            get
            {
                if (m_files == null)
                {
                    m_files = Engine.Object.Construct();

                    foreach (var file in Request.Files)
                    {
                        var content = new Base64EncodedByteArrayInstance(Engine.Object.InstancePrototype, file.Value.Content)
                          {
                              FileName = file.Value.FileName,
                              MimeType = file.Value.ContentType
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
                // ReSharper disable CoVariantArrayConversion
                var result = Engine.Array.Construct(Request.Files.Select(f => f.Key).ToArray());
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
                    result.SetPropertyValue(key, Request.Headers[key], true);
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
                var result = Engine.Object.Construct();
                foreach (var key in Request.Form.Keys)
                {
                    result.SetPropertyValue(key, Request.Form[key], false);
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

        [JSProperty(Name = "rawUrl")]
        public string RawUrl
        {
            get
            {
                return Request.Url.ToString();
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
            get { return Request.UrlReferrer.PathAndQuery; }
        }

        [JSProperty(Name = "referrer")]
        public string Referrer
        {
            get { return Request.UrlReferrer.ToString(); }
        }

        [JSProperty(Name = "method")]
        public string Method
        {
            get { return Request.Method; }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get { return Request.Path; }
        }

        [JSProperty(Name = "userAgent")]
        public string UserAgent
        {
            get { return Request.UserAgent; }
        }

        [JSProperty(Name = "clientInfo")]
        public ClientInfoInstance BrowserInfo
        {
            get
            {
                var br = BrowserUserAgentParser.GetDefault();
                var clientInfo = br.Parse(Request.UserAgent);
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
    }
}