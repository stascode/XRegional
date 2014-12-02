using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XRegional.Wrappers;

namespace XRegional.Serializers.JsonExt
{
    internal class EntityPropertyConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            EntityProperty ep = value as EntityProperty;
            if (ep == null)
                return;

            writer.WriteStartObject();
            writer.WritePropertyName(ep.PropertyType.ToString());
            serializer.Serialize(writer, TableConvert.FromEntityProperty(ep));
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            var properties = jsonObject.Properties().ToList();

            var val = properties[0].Value;
            switch (properties[0].Name)
            {
                case "Binary":
                    return new EntityProperty((byte[]) val);
                case "Boolean":
                    return new EntityProperty((bool)val);
                case "DateTime":
                    return new EntityProperty((DateTime)val);
                case "Double":
                    return new EntityProperty((double)val);
                case "Guid":
                    return new EntityProperty((Guid)val);
                case "Int32":
                    return new EntityProperty((int)val);
                case "Int64":
                    return new EntityProperty((long)val);
                case "String":
                    return new EntityProperty((string)val);
                default:
                    return null;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (EntityProperty);
        }
    }
}