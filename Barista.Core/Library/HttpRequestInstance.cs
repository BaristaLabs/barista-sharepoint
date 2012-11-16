namespace Barista.Library
{
  using System.Net;
  using Jurassic;
  using Jurassic.Library;
  using System.Web;
  using System.ServiceModel.Web;
  using System.Text;

  public class HttpRequestInstance : ObjectInstance
  {
    private BrewRequest m_request;
    private ObjectInstance m_files = null;

    public HttpRequestInstance(ScriptEngine engine, BrewRequest request)
      : base(engine)
    {
      this.m_request = request;
      this.PopulateFields();
      this.PopulateFunctions();
    }

    #region Properties

    public BrewRequest Request
    {
      get { return m_request; }
    }

    [JSProperty(Name = "accept")]
    public ArrayInstance Accept
    {
      get { return this.Engine.Array.Construct(this.m_request.AcceptTypes); }
    }

    [JSProperty(Name="contentType")]
    public string ContentType
    {
      get { return this.m_request.ContentType; }
    }

    [JSProperty(Name = "files", IsEnumerable = true)]
    public ObjectInstance Files
    {
      get
      {
        if (m_files == null)
        {
          m_files = this.Engine.Object.Construct();

          foreach (var file in this.m_request.Files)
          {
            var content = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, file.Value.Content);
            content.FileName = file.Value.FileName;
            content.MimeType = file.Value.ContentType;

            m_files.SetPropertyValue(file.Key, content, false);
          }
        }

        return m_files;
      }
    }

    [JSProperty(Name = "headers")]
    public ObjectInstance Headers
    {
      get
      {
        var result = this.Engine.Object.Construct();
        foreach (var key in this.m_request.Headers.Keys)
        {
          result.SetPropertyValue(key, this.m_request.Headers[key], true);
        }
        return result;
      }
    }

    [JSProperty(Name = "location")]
    public string Location
    {
      get { return this.m_request.Url.PathAndQuery; }
    }

    [JSProperty(Name = "form")]
    public ObjectInstance Form
    {
      get
      {
        var result = this.Engine.Object.Construct();
        foreach (var key in this.m_request.Form.Keys)
        {
          result.SetPropertyValue(key, this.m_request.Form[key], false);
        }
        return result;
      }
    }

    [JSProperty(Name = "body")]
    public Base64EncodedByteArrayInstance Body
    {
      get
      {
        return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, this.m_request.Body);
      }
    }

    [JSProperty(Name = "queryString")]
    public ObjectInstance QueryString
    {
      get
      {
        var result = this.Engine.Object.Construct();
        foreach (var key in this.m_request.QueryString.Keys)
        {
          result.SetPropertyValue(key, this.m_request.QueryString[key], false);
        }
        return result;
      }
    }

    [JSProperty(Name = "referrerLocation")]
    public string ReferrerLocation
    {
      get { return this.m_request.UrlReferrer.PathAndQuery; }
    }

    [JSProperty(Name = "referrer")]
    public string Referrer
    {
      get { return this.m_request.UrlReferrer.ToString(); }
    }

    [JSProperty(Name = "method")]
    public string Method
    {
      get { return this.m_request.Method; }
    }

    [JSProperty(Name = "url")]
    public string Url
    {
      get { return this.m_request.Path; }
    }

    [JSProperty(Name = "userAgent")]
    public string UserAgent
    {
      get { return this.m_request.UserAgent; }
    }

    #endregion

    [JSFunction(Name = "getBodyObject")]
    public object GetBodyObject()
    {
      var stringBody = Encoding.UTF8.GetString(this.m_request.Body);
      return JSONObject.Parse(this.Engine, stringBody, null);
    }
  }
}