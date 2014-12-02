namespace XRegional.Table
{
    public interface ITargetTableResolver
    {
        TargetTable Resolve(string key);
    }
}
