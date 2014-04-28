using System.Globalization;

namespace Barista
{
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
      this.AutoDetectContentType = true;
      this.Cookies = new Dictionary<string, string>();
      this.ContentEncoding = Encoding.UTF8;
      this.ContentLength = 0;
      this.ContentType = "application/json";
      this.ETag = String.Empty;
      this.Expires = 0;
      this.Headers = new Dictionary<string, string>();
      this.LastModified = DateTime.UtcNow;
      this.RedirectLocation = String.Empty;
      this.StatusCode = HttpStatusCode.OK;
      this.StatusDescription = "OK";
      this.SuppressContent = false;
    }

    [DataMember]
    public bool AutoDetectContentType
    {
      get;
      set;
    }

    [DataMember]
    public IDictionary<string, string> Cookies
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
    public Encoding ContentEncoding
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
    public string ContentType
    {
      get;
      set;
    }

    [DataMember]
    public string ETag
    {
      get;
      set;
    }

    [DataMember]
    public int Expires
    {
      get;
      set;
    }

    [DataMember]
    public IDictionary<string, string> Headers
    {
      get;
      set;
    }

    [DataMember]
    public DateTime LastModified
    {
      get;
      set;
    }

    [DataMember]
    public string RedirectLocation
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

    [NonSerialized]
    private ExtensionDataObject m_extensionData;

    public ExtensionDataObject ExtensionData
    {
      get { return m_extensionData; }
      set { m_extensionData = value; }
    }

    public void ModifyWebOperationContext(OutgoingWebResponseContext response)
    {
      response.ContentLength = this.ContentLength;
      response.ContentType = this.ContentType;
      response.ETag = this.ETag;
      response.LastModified = this.LastModified;
      response.Location = this.RedirectLocation;
      response.StatusCode = this.StatusCode;

      //Setting the status description on the outgoing web response context in certain situations causes the request to terminate unexpectedly.
      //I've tried removing new-line characters and truncating to 512 characters but both don't seem to work.
      //Since the description is being set in the httpresponse anyway, and that seems to work, I'm commenting this out.
      //response.StatusDescription = this.StatusDescription;
      //response.StatusDescription = response.StatusDescription.Substring(0,
      //  response.StatusDescription.Length > 512 ? 512 : response.StatusDescription.Length);

      response.SuppressEntityBody = this.SuppressContent;

      foreach (var header in this.Headers.Keys)
      {
        if (response.Headers.AllKeys.Any(k => k == header))
          response.Headers.Set(header, this.Headers[header]);
        else
          response.Headers.Add(header, this.Headers[header]);
      }
    }

    public void ModifyHttpResponse(HttpResponse response, bool setHeaders)
    {
      response.ContentEncoding = this.ContentEncoding;
      response.ContentType = this.ContentType;

      //Last Modified cannot be in the future. May occur if server times are not synchronized.
      if (this.LastModified.ToUniversalTime() <= DateTime.UtcNow)
        response.Cache.SetLastModified(this.LastModified.ToUniversalTime());

      response.Expires = this.Expires;
      response.RedirectLocation = this.RedirectLocation;
      response.StatusCode = (int)this.StatusCode;
      if (setHeaders)
        response.StatusDescription = this.StatusDescription;
      response.SuppressContent = this.SuppressContent;

      foreach (var cookieName in this.Cookies.Keys)
      {
        if (response.Cookies.AllKeys.Any(k => k == cookieName))
          response.Cookies.Set(new HttpCookie(cookieName, this.Cookies[cookieName]));
        else
          response.Cookies.Add(new HttpCookie(cookieName, this.Cookies[cookieName]));
      }

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
    }

    /// <summary>
    /// Sets the contents property based on the contents of the result object
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="result"></param>
    /// <param name="isRaw"></param>
    public void SetContentsFromResultObject(ScriptEngine engine, object result, bool isRaw)
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
      else if (result is StringInstance || result is string)
      {
        var stringResult = result as string ?? String.Empty;

        byteArray = Encoding.UTF8.GetBytes(stringResult);
      }
      else
      {
        //Obtain the script result and stringify it -- e.g. convert it to a json object.
        var stringResult = JSONObject.Stringify(engine, result, null, null);
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
      else if (result is StringInstance || result is string)
      {
        var stringResult = result as string;
        if (stringResult != null && (stringResult.TrimStart().StartsWith("<html", StringComparison.InvariantCultureIgnoreCase) ||
                                     stringResult.TrimStart().StartsWith("<!doctype html>", StringComparison.InvariantCultureIgnoreCase)))
          contentType = "text/html";
        else if (stringResult != null && stringResult.TrimStart().StartsWith("<?xml", StringComparison.InvariantCultureIgnoreCase))
          contentType = "text/xml";
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
