namespace Barista.Extensions
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using Barista.Newtonsoft.Json;
  using Barista.Newtonsoft.Json.Bson;
  using Barista.Newtonsoft.Json.Linq;
  using Barista.Newtonsoft.Json.Serialization;

  /// <summary>
  /// Json extensions 
  /// </summary>
  public static class JsonExtensions
  {

    /// <summary>
    /// Convert a byte array to a RavenJObject
    /// </summary>
    public static JObject ToJObject(this byte[] self)
    {
      return JObject.Load(new BsonReader(new MemoryStream(self))
      {
        DateTimeKindHandling = DateTimeKind.Utc,
      });
    }

    /// <summary>
    /// Convert a byte array to a RavenJObject
    /// </summary>
    public static JObject ToJObject(this Stream self)
    {
      return JObject.Load(new BsonReader(self)
      {
        DateTimeKindHandling = DateTimeKind.Utc,
      });
    }

    /// <summary>
    /// Convert a JToken to a byte array
    /// </summary>
    public static void WriteTo(this JToken self, Stream stream)
    {
      self.WriteTo(new BsonWriter(stream)
      {
        DateTimeKindHandling = DateTimeKind.Utc
      });
    }

    /// <summary>
    /// Deserialize a <param name="self"/> to an instance of <typeparam name="T"/>
    /// </summary>
    public static T JsonDeserialization<T>(this byte[] self)
    {
      return (T)CreateDefaultJsonSerializer().Deserialize(new BsonReader(new MemoryStream(self)), typeof(T));
    }

    /// <summary>
    /// Deserialize a <param name="self"/> to an instance of<typeparam name="T"/>
    /// </summary>
    public static T JsonDeserialization<T>(this JObject self)
    {
      return (T)CreateDefaultJsonSerializer().Deserialize(new JTokenReader(self), typeof(T));
    }

    /// <summary>
    /// Deserialize a <param name="self"/> to an instance of<typeparam name="T"/>
    /// </summary>
    public static T JsonDeserialization<T>(this StreamReader self)
    {
      return CreateDefaultJsonSerializer().Deserialize<T>(self);
    }

    public static T Deserialize<T>(this JsonSerializer self, TextReader reader)
    {
      return (T)self.Deserialize(reader, typeof(T));
    }

    private static readonly IContractResolver ContractResolver = new DefaultServerContractResolver(true)
    {
      DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
    };

    private class DefaultServerContractResolver : DefaultContractResolver
    {
      public DefaultServerContractResolver(bool shareCache)
        : base(shareCache)
      {
      }

      protected override System.Collections.Generic.List<MemberInfo> GetSerializableMembers(Type objectType)
      {
        var serializableMembers = base.GetSerializableMembers(objectType);
        foreach (var toRemove in serializableMembers
          .Where(MembersToFilterOut)
          .ToArray())
        {
          serializableMembers.Remove(toRemove);
        }
        return serializableMembers;
      }

      private static bool MembersToFilterOut(MemberInfo info)
      {
        if (info is EventInfo)
          return true;
        var fieldInfo = info as FieldInfo;
        if (fieldInfo != null && !fieldInfo.IsPublic)
          return true;
        return info.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length > 0;
      }
    }

    public static JsonSerializer CreateDefaultJsonSerializer()
    {
      var jsonSerializer = new JsonSerializer
      {
        DateParseHandling = DateParseHandling.DateTime,
        ContractResolver = ContractResolver
      };
      
      return jsonSerializer;
    }
  }
}
