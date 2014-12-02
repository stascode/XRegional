namespace XRegional
{
    public interface IGatewayBlobStore
    {
        string Write(byte[] packed);

        byte[] Read(string uri);
    }
}
