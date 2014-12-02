using Newtonsoft.Json;

namespace XRegional.Docdb
{
    public class VersionedDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "_etag")]
        public string ETag { get; internal set; }
        [JsonProperty(PropertyName = "_self")]
        public string SelfLink { get; internal set; }

        public long Version { get; set; }
    }
}
