namespace Barista.Library
{
  using System.Net;
  using Jurassic;
  using Jurassic.Library;
  using System.Web;
  using System.ServiceModel.Web;
  using System.Text;
  using System;

  [Serializable]
  public class HttpRequestInstance : ObjectInstance
  {
    private ObjectInstance m_files = null;

    public HttpRequestInstance(ScriptEngine engine, BrewRequest request)
      : base(engine)
    {
      this.Request = request;
      this.PopulateFields();
      this.PopulateFunctions();
    }

    #region Properties

    public BrewRequest Request
    {
      get;
      private set;
    }

    [JSProperty(Name = "accept")]
    public ArrayInstance Accept
    {
      get { return this.Engine.Array.Construct(this.Request.AcceptTypes); }
    }

    [JSProperty(Name="contentType")]
    public string ContentType
    {
      get { return this.Request.ContentType; }
    }

    [JSProperty(Name = "files", IsEnumerable = true)]
    public ObjectInstance Files
    {
      get
      {
        if (m_files == null)
        {
          m_files = this.Engine.Object.Construct();

          foreach (var file in this.Request.Files)
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
        foreach (var key in this.Request.Headers.Keys)
        {
          result.SetPropertyValue(key, this.Request.Headers[key], true);
        }
        return result;
      }
    }

    [JSProperty(Name = "location")]
    public string Location
    {
      get { return this.Request.Url.PathAndQuery; }
    }

    [JSProperty(Name = "form")]
    public ObjectInstance Form
    {
      get
      {
        var result = this.Engine.Object.Construct();
        foreach (var key in this.Request.Form.Keys)
        {
          result.SetPropertyValue(key, this.Request.Form[key], false);
        }
        return result;
      }
    }

    [JSProperty(Name = "body")]
    public Base64EncodedByteArrayInstance Body
    {
      get
      {
        return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, this.Request.Body);
      }
    }

    [JSProperty(Name = "queryString")]
    public ObjectInstance QueryString
    {
      get
      {
        var result = this.Engine.Object.Construct();
        foreach (var key in this.Request.QueryString.Keys)
        {
          result.SetPropertyValue(key, this.Request.QueryString[key], false);
        }
        return result;
      }
    }

    [JSProperty(Name = "referrerLocation")]
    public string ReferrerLocation
    {
      get { return this.Request.UrlReferrer.PathAndQuery; }
    }

    [JSProperty(Name = "referrer")]
    public string Referrer
    {
      get { return this.Request.UrlReferrer.ToString(); }
    }

    [JSProperty(Name = "method")]
    public string Method
    {
      get { return this.Request.Method; }
    }

    [JSProperty(Name = "url")]
    public string Url
    {
      get { return this.Request.Path; }
    }

    [JSProperty(Name = "userAgent")]
    public string UserAgent
    {
      get { return this.Request.UserAgent; }
    }

    #endregion

    [JSFunction(Name = "getBodyObject")]
    public object GetBodyObject()
    {
      var stringBody = Encoding.UTF8.GetString(this.Request.Body);
      return JSONObject.Parse(this.Engine, stringBody, null);
    }
  }
}