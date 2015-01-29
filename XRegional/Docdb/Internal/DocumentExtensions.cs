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

        // Sets Version to 1 if it does not exist / its value is equal to 0
        public static void InitVersion(this Document document)
        {
            long value = document.GetVersion();
            if (value == 0)
                document.SetVersion(1);
        }
    }
}
