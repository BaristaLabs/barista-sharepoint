using Barista.Newtonsoft.Json;
namespace Barista
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class BrewRequestHeadersJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var items = value as IEnumerable<KeyValuePair<string, IEnumerable<string>>>;
            writer.WriteStartArray();
            if (items != null)
            {
                foreach (var item in items)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName(item.Key);
                    if (item.Value.Count() == 1)
                        writer.WriteValue(item.Value.First());
                    else
                        serializer.Serialize(writer, item.Value);
                    writer.WriteEndObject();
                }
            }
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IEnumerable<KeyValuePair<string, IEnumerable<string>>>);
        }
    }
}
