using System.IO;
using System.IO.Compression;
using System.Text;
using XRegional.Common;

namespace XRegional.Serializers
{
    public static class ZipCompressor
    {
        public static byte[] Compress<T>(T obj)
        {
            Guard.NotNull(obj, "obj");

            byte[] serialized = Encoding.UTF8.GetBytes(JsonCustomConvert.SerializeObject(obj));

            using (MemoryStream outStream = new MemoryStream())
            {
                using (var gzipped = new GZipStream(outStream, CompressionMode.Compress))
                using (var ms = new MemoryStream(serialized))
                    ms.CopyTo(gzipped);

                return outStream.ToArray();
            }
        }

        public static T Uncompress<T>(byte[] content)
        {
            Guard.NotNull(content, "content");

            using (var contentStream = new MemoryStream(content))
            using (var unzippedStream = new GZipStream(contentStream, CompressionMode.Decompress))
            using (var outStream = new MemoryStream())
            {
                unzippedStream.CopyTo(outStream);

                byte[] unzipped = outStream.ToArray();

                return JsonCustomConvert.DeserializeObject<T>(Encoding.UTF8.GetString(unzipped));
            }
        }
    }
}
