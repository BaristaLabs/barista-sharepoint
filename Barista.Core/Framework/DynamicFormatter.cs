namespace Barista.Framework
{
  using System;
  using System.Collections.Specialized;
  using System.Net;
  using System.ServiceModel;
  using System.ServiceModel.Channels;
  using System.ServiceModel.Dispatcher;
  using System.Web;


  /// <summary>
  /// Represents a WCF formatter that serializes the Web response in either Json or Xml based on the accept-header or the presence of a $format query string.
  /// </summary>
  public class DynamicFormatter : IDispatchMessageFormatter
  {
    public IDispatchMessageFormatter JsonDispatchMessageFormatter
    {
      get;
      set;
    }

    public IDispatchMessageFormatter XmlDispatchMessageFormatter
    {
      get;
      set;
    }

    public void DeserializeRequest(System.ServiceModel.Channels.Message message, object[] parameters)
    {
      throw new NotImplementedException();
    }

    public System.ServiceModel.Channels.Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
    {
      Message request = OperationContext.Current.RequestContext.RequestMessage;

      // This code is based on ContentTypeBasedDispatch example in WCF REST Starter Kit Samples
      // It calls either 
      HttpRequestMessageProperty prop = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
      NameValueCollection queryString = HttpUtility.ParseQueryString(prop.QueryString);
      string accepts = prop.Headers[HttpRequestHeader.Accept];
      if (queryString["$format"] != null)
      {
        if (queryString["$format"] == "xml")
        {
          return XmlDispatchMessageFormatter.SerializeReply(messageVersion, parameters, result);
        }

        if (queryString["$format"] == "json")
        {
          return JsonDispatchMessageFormatter.SerializeReply(messageVersion, parameters, result);
        }
      }
      else if (accepts != null)
      {
        if (accepts.Contains("text/xml") || accepts.Contains("application/xml") || queryString["$format"] == "xml")
        {
          return XmlDispatchMessageFormatter.SerializeReply(messageVersion, parameters, result);
        }

        if (accepts.Contains("application/json") || queryString["$format"] == "json")
        {
          return JsonDispatchMessageFormatter.SerializeReply(messageVersion, parameters, result);
        }
      }
      else
      {
        string contentType = prop.Headers[HttpRequestHeader.ContentType];
        if (contentType != null)
        {
          if (contentType.Contains("text/xml") || contentType.Contains("application/xml") || queryString["$format"] == "xml")
          {
            return XmlDispatchMessageFormatter.SerializeReply(messageVersion, parameters, result);
          }

          if (contentType.Contains("application/json") || queryString["$format"] == "json")
          {
            return JsonDispatchMessageFormatter.SerializeReply(messageVersion, parameters, result);
          }
        }
      }

      //Use JSON by default...
      return JsonDispatchMessageFormatter.SerializeReply(messageVersion, parameters, result);
    }

  }
}
