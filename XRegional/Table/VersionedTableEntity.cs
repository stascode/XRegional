using Microsoft.WindowsAzure.Storage.Table;

namespace XRegional.Table
{
    public class VersionedTableEntity : TableEntity
    {
        // 64-bit integer is hopefully enough for the majority of scenarious
        public long Version { get; set; }

        public VersionedTableEntity()
        {
            Version = 1;
        }
    }
}
