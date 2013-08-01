namespace Barista.Framework
{
  using Barista.Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.ServiceModel.Channels;
  using System.ServiceModel.Description;
  using System.ServiceModel.Dispatcher;
  using System.Web;

  public class NewtonsoftJsonDispatchFormatter : IDispatchMessageFormatter
  {
    private readonly OperationDescription m_operation;
    readonly Dictionary<string, int> m_parameterNames;
    public NewtonsoftJsonDispatchFormatter(OperationDescription operation, bool isRequest)
    {
      this.m_operation = operation;
      if (isRequest)
      {
        int operationParameterCount = operation.Messages[0].Body.Parts.Count;
        if (operationParameterCount > 1)
        {
          this.m_parameterNames = new Dictionary<string, int>();
          for (int i = 0; i < operationParameterCount; i++)
          {
            this.m_parameterNames.Add(operation.Messages[0].Body.Parts[i].Name, i);
          }
        }
      }
    }

    public void DeserializeRequest(Message message, object[] parameters)
    {
      object bodyFormatProperty;
      if (!message.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out bodyFormatProperty) ||
          (bodyFormatProperty as WebBodyFormatMessageProperty) == null ||
          (bodyFormatProperty as WebBodyFormatMessageProperty).Format != WebContentFormat.Raw)
      {
        throw new InvalidOperationException("Incoming messages must have a body format of Raw. Is a ContentTypeMapper set on the WebHttpBinding?");
      }

      var bodyReader = message.GetReaderAtBodyContents();
      bodyReader.ReadStartElement("Binary");

      var rawBody = bodyReader.ReadContentAsBase64();

      using (var ms = new MemoryStream(rawBody))
      {
        using (var sr = new StreamReader(ms))
        {
          var serializer = NewtonsoftJsonSettings.GetSerializer();

          if (parameters.Length == 1)
          {
            if (m_operation.Messages[0].Body.Parts[0].Type == typeof (string))
            {
              var str = sr.ReadToEnd();
              var queryString = HttpUtility.ParseQueryString(str);
              if (queryString.AllKeys.Contains(m_operation.Messages[0].Body.Parts[0].Name))
              {
                parameters[0] = Convert.ChangeType(queryString[m_operation.Messages[0].Body.Parts[0].Name], m_operation.Messages[0].Body.Parts[0].Type);
              }
            }
            else
              parameters[0] = serializer.Deserialize(sr, m_operation.Messages[0].Body.Parts[0].Type);
          }
          else
          {
            // multiple parameter, needs to be wrapped
            Newtonsoft.Json.JsonReader reader = new Newtonsoft.Json.JsonTextReader(sr);
            reader.Read();
            if (reader.TokenType != Newtonsoft.Json.JsonToken.StartObject)
            {
              throw new InvalidOperationException("Input needs to be wrapped in an object");
            }

            reader.Read();
            while (reader.TokenType == Newtonsoft.Json.JsonToken.PropertyName)
            {
              var parameterName = reader.Value as string;

              if (parameterName == null)
                throw new InvalidOperationException("The object contained a parameter, however the value was null.");

              reader.Read();
              if (this.m_parameterNames.ContainsKey(parameterName))
              {
                int parameterIndex = this.m_parameterNames[parameterName];
                parameters[parameterIndex] = serializer.Deserialize(reader,
                  this.m_operation.Messages[0].Body.Parts[parameterIndex].Type);
              }
              else
              {
                reader.Skip();
              }

              reader.Read();
            }

            reader.Close();
          }

        }
      }
    }

    public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
    {
      byte[] body;

      var serializer = NewtonsoftJsonSettings.GetSerializer();

      using (var ms = new MemoryStream())
      {
        using (var sw = new StreamWriter(ms))
        {
          using (Newtonsoft.Json.JsonWriter writer = new Newtonsoft.Json.JsonTextWriter(sw))
          {
            writer.Formatting = NewtonsoftJsonSettings.GetFormatting();
            serializer.Serialize(writer, result);
            sw.Flush();
            body = ms.ToArray();
          }
        }
      }
      var respProp = new HttpResponseMessageProperty();
      respProp.Headers[HttpResponseHeader.ContentType] = "application/json";

      var replyMessage = Message.CreateMessage(messageVersion, m_operation.Messages[1].Action, new RawBodyWriter(body));
      replyMessage.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Raw));
      replyMessage.Properties.Add(HttpResponseMessageProperty.Name, respProp);

      return replyMessage;
    }
  }
}
