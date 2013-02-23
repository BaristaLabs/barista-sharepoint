namespace Barista.Framework
{
  using System.IO;
  using Newtonsoft.Json;
  using RestSharp.Serializers;

  /// <summary>
  /// Default JSON serializer for request bodies
  /// Doesn't currently use the SerializeAs attribute, defers to Newtonsoft's attributes
  /// </summary>
  public class JsonNetSerializer : ISerializer
  {
    private readonly Newtonsoft.Json.JsonSerializer m_serializer;

    /// <summary>
    /// Default serializer
    /// </summary>
    public JsonNetSerializer()
    {
      this.ContentType = "application/json";
      m_serializer = new Newtonsoft.Json.JsonSerializer
      {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling = NullValueHandling.Include,
        DefaultValueHandling = DefaultValueHandling.Include,
        
        DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
      };
    }

    /// <summary>
    /// Default serializer with overload for allowing custom Json.NET settings
    /// </summary>
    public JsonNetSerializer(Newtonsoft.Json.JsonSerializer serializer)
    {
      ContentType = "application/json";
      m_serializer = serializer;
    }

    /// <summary>
    /// Serialize the object as JSON
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <returns>JSON as String</returns>
    public string Serialize(object obj)
    {
      using (var stringWriter = new StringWriter())
      {
        using (var jsonTextWriter = new JsonTextWriter(stringWriter))
        {
          jsonTextWriter.Formatting = Formatting.Indented;
          jsonTextWriter.QuoteChar = '"';

          m_serializer.Serialize(jsonTextWriter, obj);

          var result = stringWriter.ToString();
          return result;
        }
      }
    }

    /// <summary>
    /// Unused for JSON Serialization
    /// </summary>
    public string DateFormat
    {
      get;
      set;
    }
    /// <summary>
    /// Unused for JSON Serialization
    /// </summary>
    public string RootElement
    {
      get;
      set;
    }

    /// <summary>
    /// Unused for JSON Serialization
    /// </summary>
    public string Namespace
    {
      get;
      set;
    }

    /// <summary>
    /// Content type for serialized content
    /// </summary>
    public string ContentType
    {
      get;
      set;
    }
  }
}