namespace XRegional.Docdb
{
    public interface ITargetCollectionResolver
    {
        TargetCollection Resolve(string key);
    }
}
