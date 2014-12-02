using Microsoft.WindowsAzure.Storage.Table;

namespace XRegional.Table
{
    public class XTableResult
    {
        public bool Discarded { get; set; }
        public TableResult TableResult { get; set; }
    }
}
