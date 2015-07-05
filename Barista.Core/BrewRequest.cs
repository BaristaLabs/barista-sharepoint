namespace Barista
{
    using System.Collections.Specialized;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Text.RegularExpressions;
  using Barista.Extensions;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.Serialization;
  using System.Web;

  [DataContract(Namespace = Constants.ServiceNamespace)]
  [Serializable]
    public sealed class BrewRequest : IExtensibleDataObject, IDisposable
  {
    public BrewRequest()
    {
            this.Body = new byte[0];
      this.CodePath = String.Empty;
      this.ExecutionTimeout = 110000;
            this.Headers = new BrewRequestHeaders(new Dictionary<string, IEnumerable<string>>());

            this.ExtendedProperties = new Dictionary<string, string>();
    }

    #region Properties

        /// <summary>
        /// Gets or sets the original body of the request.
        /// </summary>
    [DataMember]
    public byte[] Body
    {
      get;
      set;
    }

        /// <summary>
        /// Gets or sets the code that will be executed by the service.
        /// </summary>
    [DataMember]
    public string Code
    {
      get;
      set;
    }

        /// <summary>
        /// Gets or sets the path to the code (used for indicating source location if an exception is thrown during processing)
        /// </summary>
    [DataMember]
    public string CodePath
    {
      get;
      set;
    }

        /// <summary>
        /// Gets or sets the code that will be executed by the service.
        /// </summary>
    [DataMember]
        public string InstanceInitializationCode
    {
      get;
      set;
    }

        /// <summary>
        /// Gets or sets the path to the code (used for indicating source location if an exception is thrown during processing)
        /// </summary>
    [DataMember]
        public string InstanceInitializationCodePath
    {
      get;
      set;
    }

    [DataMember]
    public int ExecutionTimeout
    {
      get;
      set;
    }

    [DataMember]
        public BrewRequestHeaders Headers
    {
      get;
      set;
    }

        /// <summary>
        /// Gets the originating http request method (GET/POST/PUT/DELETE etc...)
        /// </summary>
    [DataMember]
        public string Method
    {
      get;
      set;
    }

    [DataMember]
        public IDictionary<string, string> QueryString
    {
      get;
      set;
    }

    [DataMember]
        public string ScriptEngineFactory
    {
      get;
      set;
    }

    [DataMember]
        public Uri Url
    {
      get;
      set;
    }

    [DataMember]
        public string UserHostAddress
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
        #endregion

        private Stream m_bodyStream;
        public IDictionary<string, string> ParseFormData(out IList<HttpFile> files)
    {
            files = new List<HttpFile>();

            if (string.IsNullOrEmpty(this.Headers.ContentType))
    {
                return null;
    }

            IDictionary<string, string> formData = new Dictionary<string, string>();

            var contentType = this.Headers.ContentType;
            var mimeType = contentType.Split(';').First();
            if (mimeType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
    {
                formData = Encoding.UTF8.GetString(this.Body).AsQueryDictionary();
    }

            if (!mimeType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
    {
                return formData;
    }

            var boundary = Regex.Match(contentType, @"boundary=""?(?<token>[^\n\;\"" ]*)").Groups["token"].Value;

            if (m_bodyStream == null)
                m_bodyStream = new MemoryStream(this.Body);

            var multipart = new HttpMultipart(m_bodyStream, boundary);

            var formValues =
                new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

            foreach (var httpMultipartBoundary in multipart.GetBoundaries())
    {
                if (string.IsNullOrEmpty(httpMultipartBoundary.Filename))
    {
                    var reader =
                        new StreamReader(httpMultipartBoundary.Value);
                    formValues.Add(httpMultipartBoundary.Name, reader.ReadToEnd());

    }
                else
    {
                    files.Add(new HttpFile(httpMultipartBoundary));
    }
    }

            foreach (var key in formValues.AllKeys.Where(key => key != null))
    {
                formData[key] = formValues[key];
    }

            m_bodyStream.Position = 0;
            return formData;
        }

        public bool ShouldForceStrict()
    {
            const string forceStrictKey = "barista_forcestrict";
            if (
                (QueryString != null && QueryString.Keys.Any(k => k.IsNullOrWhiteSpace() == false && k.ToLowerInvariant() == forceStrictKey)) ||
                (Headers != null && Headers.Keys.Any(k => k.IsNullOrWhiteSpace() == false && k.ToLowerInvariant() == forceStrictKey)))
                return true;

            return false;
    }

        /// <summary>
        /// Returns the barista instance initialization code param from the query string or headers.
        /// </summary>
        /// <returns></returns>
        public BrewRequestInstanceSettings ParseInstanceSettings()
    {
            var instanceSettings = new BrewRequestInstanceSettings();
            string instanceInitializationCodeKey;
            string instanceModeKey;
            string instanceNameKey;
            string instanceAbsoluteExpirationKey;
            string instanceSlidingExpirationKey;

            var isSet = false;

            if (Headers != null)
    {
                //Instance Initialization Code
                instanceInitializationCodeKey = Headers.Keys.FirstOrDefault(k => k.IsNullOrWhiteSpace() == false && k.ToLowerInvariant() == "barista_instanceinitializationcode");
                if (instanceInitializationCodeKey != null)
    {
                    instanceSettings.InstanceInitializationCode = Headers[instanceInitializationCodeKey].FirstOrDefault();
    }

                //InstanceMode
                instanceModeKey = Headers.Keys.FirstOrDefault(k => k.IsNullOrWhiteSpace() == false && k.ToLowerInvariant() == "barista_instancemode");
                if (instanceModeKey != null)
    {
                    BaristaInstanceMode instanceMode;
                    if (Headers[instanceModeKey].FirstOrDefault().TryParseEnum(true, out instanceMode))
                        instanceSettings.InstanceMode = instanceMode;
                    else
                        throw new InvalidOperationException(
                            "Unable to determine the instance mode from the request header. Possible instance modes are PerSession, PerCall, and Single. " +
                            Headers[instanceModeKey].FirstOrDefault());

                    if (instanceMode == BaristaInstanceMode.Single || instanceMode == BaristaInstanceMode.PerSession)
    {
                        instanceNameKey =
                            Headers.Keys.FirstOrDefault(
                                k => k.IsNullOrWhiteSpace() == false && k.ToLowerInvariant() == "barista_instancename");
                        if (instanceNameKey == null)
                            throw new InvalidOperationException(
                                "If a Barista Instance Mode of Single or Per-Sesson is specified, an Instance Name must also be specified.");

                        instanceSettings.InstanceName = Headers[instanceNameKey].FirstOrDefault();

                        instanceAbsoluteExpirationKey =
                            Headers.Keys.FirstOrDefault(
                                k =>
                                    k.IsNullOrWhiteSpace() == false &&
                                    k.ToLowerInvariant() == "barista_instanceabsoluteexpiration");
                        if (instanceAbsoluteExpirationKey != null)
    {
                            instanceSettings.InstanceAbsoluteExpiration =
                                DateTime.Parse(Headers[instanceAbsoluteExpirationKey].FirstOrDefault());
        }

                        instanceSlidingExpirationKey =
                            Headers.Keys.FirstOrDefault(
                                k =>
                                    k.IsNullOrWhiteSpace() == false &&
                                    k.ToLowerInvariant() == "barista_instanceslidingexpiration");
                        if (instanceSlidingExpirationKey != null)
        {
                            instanceSettings.InstanceSlidingExpiration =
                                TimeSpan.Parse(Headers[instanceSlidingExpirationKey].FirstOrDefault());
        }

      }

                    isSet = true;
      }
      }

            if (QueryString != null && !isSet)
      {
                instanceInitializationCodeKey =
                    QueryString.Keys.FirstOrDefault(
                        k =>
                            k.IsNullOrWhiteSpace() == false &&
                            k.ToLowerInvariant() == "barista_instanceinitializationcode");

                if (instanceInitializationCodeKey != null)
      {
                    instanceSettings.InstanceInitializationCode = QueryString[instanceInitializationCodeKey];
      }

                instanceModeKey = QueryString.Keys.FirstOrDefault(k => k.IsNullOrWhiteSpace() == false && k.ToLowerInvariant() == "barista_instancemode");
      if (instanceModeKey != null)
      {
        BaristaInstanceMode instanceMode;
                    if (QueryString[instanceModeKey].TryParseEnum(true, out instanceMode))
                        instanceSettings.InstanceMode = instanceMode;
        else
                        throw new InvalidOperationException("Unable to determine the instance mode from the query string. Possible instance modes are PerSession, PerCall, and Single. " + QueryString[instanceModeKey]);

                    if (instanceMode == BaristaInstanceMode.Single || instanceMode == BaristaInstanceMode.PerSession)
        {
                        instanceNameKey = QueryString.Keys.FirstOrDefault(k => k.IsNullOrWhiteSpace() == false && k.ToLowerInvariant() == "barista_instancename");

          if (instanceNameKey == null)
            throw new InvalidOperationException("If a Barista Instance Mode of Single or Per-Sesson is specified, an Instance Name must also be specified.");

                        instanceSettings.InstanceName = QueryString[instanceNameKey];

                        instanceAbsoluteExpirationKey = QueryString.Keys.FirstOrDefault(k => k.IsNullOrWhiteSpace() == false && k.ToLowerInvariant() == "barista_instanceabsoluteexpiration");
          if (instanceAbsoluteExpirationKey != null)
          {
                            instanceSettings.InstanceAbsoluteExpiration = DateTime.Parse(QueryString[instanceAbsoluteExpirationKey]);
          }

                        instanceSlidingExpirationKey = QueryString.Keys.FirstOrDefault(k => k.IsNullOrWhiteSpace() == false && k.ToLowerInvariant() == "barista_instanceslidingexpiration");
          if (instanceSlidingExpirationKey != null)
          {
                            instanceSettings.InstanceSlidingExpiration = TimeSpan.Parse(QueryString[instanceSlidingExpirationKey]);
                        }
                    }
                }
          }

            return instanceSettings;
      }

        public void Dispose()
          {
            if (m_bodyStream != null)
                ((IDisposable)m_bodyStream).Dispose();
        }

        #region Static Methods

        public static BrewRequest CreateServiceApplicationRequestFromHttpRequest(HttpRequest request)
        {

            var result = new BrewRequest
            {
                Method = request.HttpMethod,
                Url = request.Url,
                QueryString = request.Url.Query.AsQueryDictionary(),
                Body = request.InputStream.ToByteArray(),
                Headers = new BrewRequestHeaders(request.Headers.ToDictionary2()),
                UserHostAddress = request.UserHostAddress
            };

            return result;
            }

        public static BrewRequest CreateBrewRequestFromIncomingWebRequest(IncomingWebRequestContext webRequest, Stream requestBody, OperationContext context)
            {
            string ip = null;

            object remoteEndpointProperty;
            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(RemoteEndpointMessageProperty.Name, out remoteEndpointProperty))
      {
                ip = ((RemoteEndpointMessageProperty)remoteEndpointProperty).Address;
      }

            var request = new BrewRequest
        {
                Method = webRequest.Method,
                Url = webRequest.UriTemplateMatch.RequestUri,
                QueryString = webRequest.UriTemplateMatch.RequestUri.Query.AsQueryDictionary(),
                Body = requestBody.ToByteArray(),
                Headers = new BrewRequestHeaders(webRequest.Headers.ToDictionary2()),
                UserHostAddress = ip
            };

            return request;
    }
    #endregion
  }
}
