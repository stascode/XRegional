using System;
using System.IO;
using System.Text;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace XRegional.Serializers.JsonExt
{
    internal class DocumentConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Document jsobj = value as Document;
            if (jsobj == null)
                return;

            using (var ms = new MemoryStream())
            {
                jsobj.SaveTo(ms);
                string json = Encoding.UTF8.GetString( ms.ToArray() );
                writer.WriteRawValue(json);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Document document = new Document();
            document.LoadFrom(reader);
            return document;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Document);
        }
    }
}