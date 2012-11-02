namespace Barista
{
  using System;
  using System.Collections.Generic;
  using Barista.Extensions;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Runtime.Serialization;
  using System.ServiceModel.Web;
  using System.Text;
  using System.Web;
  using Jurassic.Library;
  using Jurassic;
  using Barista.Library;

  [DataContract(Namespace = Constants.ServiceNamespace)]
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
      this.LastModified = DateTime.Now;
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

    public ExtensionDataObject ExtensionData
    {
      get;
      set;
    }

    public void ModifyWebOperationContext(OutgoingWebResponseContext response)
    {
      response.ContentLength = this.ContentLength;
      response.ContentType = this.ContentType;
      response.ETag = this.ETag;
      response.LastModified = this.LastModified;
      response.Location = this.RedirectLocation;
      response.StatusCode = this.StatusCode;
      response.StatusDescription = this.StatusDescription;
      response.SuppressEntityBody = this.SuppressContent;

      foreach(var header in this.Headers.Keys)
      {
        if (response.Headers.AllKeys.Any(k => k == header))
          response.Headers.Set(header, this.Headers[header]);
        else
          response.Headers.Add(header, this.Headers[header]);
      }
    }

    public void ModifyHttpResponse(HttpResponse response)
    {
      response.ContentEncoding = this.ContentEncoding;
      response.ContentType = this.ContentType;
      response.Expires = this.Expires;
      response.RedirectLocation = this.RedirectLocation;
      //response.Status
      response.StatusCode = (int)this.StatusCode;
      response.StatusDescription = this.StatusDescription;
      response.SuppressContent = this.SuppressContent;

      foreach (var cookieName in this.Cookies.Keys)
      {
        if (response.Cookies.AllKeys.Any(k => k == cookieName))
          response.Cookies.Set(new HttpCookie(cookieName, this.Cookies[cookieName]));
        else
          response.Cookies.Add(new HttpCookie(cookieName, this.Cookies[cookieName]));
      }

      foreach(var header in this.Headers.Keys)
      {
        if (response.Headers.AllKeys.Any(k => k == header))
          response.Headers.Set(header, this.Headers[header]);
        else
          response.Headers.Add(header, this.Headers[header]);
      }
    }

    /// <summary>
    /// Sets the contents property based on the contents of the result object
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="result"></param>
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
          byteArray = Encoding.UTF8.GetBytes(result as string);
        }
        else
        {
          byteArray = StringHelper.StringToByteArray(result.ToString());
        }
      }
      else if (result is Base64EncodedByteArrayInstance)
      {
        byteArray = (result as Base64EncodedByteArrayInstance).Data;
      }
      else if (result is StringInstance || result is string)
      {
        byteArray = Encoding.UTF8.GetBytes(result as string);
      }
      else
      {
        //Obtain the script result and stringify it -- e.g. convert it to a json object.
        string stringResult = JSONObject.Stringify(engine, result);
        if (String.IsNullOrEmpty(stringResult) || (stringResult != null && String.IsNullOrEmpty(stringResult.Trim())))
        {
          byteArray = new byte[0];
        }
        else
        {
          byteArray = Encoding.UTF8.GetBytes(stringResult.ToString());
        }
      }

      this.Content = byteArray;
    }


    /// <summary>
    /// Determines the content type from the specified result.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static string AutoDetectContentTypeFromResult(object result)
    {
      var contentType = "application/json";

      if (result is Base64EncodedByteArrayInstance)
      {
        var base64EncodedByteArray = result as Base64EncodedByteArrayInstance;
        contentType = base64EncodedByteArray.MimeType;
      }
      else if (result is StringInstance || result is string)
      {
        var stringResult = result as string;
        if (stringResult.TrimStart().StartsWith("<html", StringComparison.InvariantCultureIgnoreCase) ||
            stringResult.TrimStart().StartsWith("<!doctype html>", StringComparison.InvariantCultureIgnoreCase))
          contentType = "text/html";
        else if (stringResult.TrimStart().StartsWith("<?xml", StringComparison.InvariantCultureIgnoreCase))
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
