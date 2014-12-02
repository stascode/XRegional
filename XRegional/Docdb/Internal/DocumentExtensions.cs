using Microsoft.Azure.Documents;

namespace XRegional.Docdb.Internal
{
    internal static class DocumentExtensions
    {
        public static long GetVersion(this Document document)
        {
            return document.GetPropertyValue<long>("Version");
        }

        public static void SetVersion(this Document document, long value)
        {
            document.SetPropertyValue("Version", value);
        }
    }
}
