using System;
using Microsoft.Azure.Documents;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace XRegional.Serializers.JsonExt
{
    internal class CustomContractResolver : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (objectType == typeof (EntityProperty))
                return new EntityPropertyConverter();

            if (objectType.IsSubclassOf(typeof(Document)) ||
                objectType == typeof (Document))
                return new DocumentConverter();

            return base.ResolveContractConverter(objectType);
        }
    }
}
