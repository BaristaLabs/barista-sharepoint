namespace Barista
{
  using System;
  using System.Globalization;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Barista.Newtonsoft.Json;
  using Barista.Newtonsoft.Json.Utilities;

  public class ObjectInstanceConverter : JsonConverter
  {
    public ObjectInstanceConverter(ScriptEngine engine)
    {
      if (engine == null)
        throw new ArgumentNullException("engine");
      this.Engine = engine;
    }

    private ScriptEngine Engine
    {
      get;
      set;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var json = JSONObject.Stringify(Engine, value, null, null);
      writer.WriteRawValue(json);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      return ReadValue(reader);
    }

    public override bool CanConvert(Type objectType)
    {
      return typeof(ArrayInstance).IsAssignableFrom(objectType);
    }

    private object ReadValue(JsonReader reader)
    {
      while (reader.TokenType == JsonToken.Comment)
      {
        if (!reader.Read())
          throw JsonSerializationException.Create(reader, "Unexpected end when reading ObjectInstance.");
      }

      switch (reader.TokenType)
      {
        case JsonToken.StartObject:
          return ReadObject(reader);
        case JsonToken.StartArray:
          return ReadArray(reader);
        case JsonToken.Date:
          var dateInstance = JurassicHelper.ToDateInstance(this.Engine, (DateTime) reader.Value);
          return dateInstance;
        case JsonToken.Null:
          return Null.Value;
        case JsonToken.Undefined:
          return Undefined.Value;
        case JsonToken.Bytes:
          var byteArray = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, (byte[]) reader.Value);
          return byteArray;
        case JsonToken.Boolean:
        case JsonToken.Float:
        case JsonToken.Integer:
        case JsonToken.String:
          return reader.Value;
        default:
          throw JsonSerializationException.Create(reader, "Unexpected token when converting ObjectInstance: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
      }
    }

    private object ReadArray(JsonReader reader)
    {
      var array = this.Engine.Array.Construct();

      while (reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.Comment:
            break;
          default:
            var v = ReadValue(reader);

            ArrayInstance.Push(array, v);
            break;
          case JsonToken.EndArray:
            return array;
        }
      }

      throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
    }

    private object ReadObject(JsonReader reader)
    {
      var instance = this.Engine.Object.Construct();

      while (reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            var propertyName = reader.Value.ToString();

            if (!reader.Read())
              throw JsonSerializationException.Create(reader, "Unexpected end when reading ObjectInstance.");

            object v = ReadValue(reader);

            instance.SetPropertyValue(propertyName, v, false);
            break;
          case JsonToken.Comment:
            break;
          case JsonToken.EndObject:
            return instance;
        }
      }

      throw JsonSerializationException.Create(reader, "Unexpected end when reading ObjectInstance.");
    }
  }
}
