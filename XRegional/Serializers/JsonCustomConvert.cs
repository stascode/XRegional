using Newtonsoft.Json;
using XRegional.Common;
using XRegional.Serializers.JsonExt;

namespace XRegional.Serializers
{
    /// <summary>
    /// Serializes and deserializes GatewayMessages and other objects respecting 
    /// EntityProperty and Document types
    /// </summary>
    public static class JsonCustomConvert
    {
        public static string SerializeObject<T>(T obj)
        {
            Guard.NotNull(obj, "obj");

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver()
            };

            return JsonConvert.SerializeObject(obj, settings);
        }


        public static T DeserializeObject<T>(string stringContent)
        {
            Guard.NotNullOrEmpty(stringContent, "stringContent");

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver()
            };

            return JsonConvert.DeserializeObject<T>(stringContent, settings);
        }
    }
}
