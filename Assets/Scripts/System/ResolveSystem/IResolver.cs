namespace System.ResolveSystem
{
    public interface IResolver
    {
        TType Resolve<TType>();
        TType Resolve<TType>(string id);
    }
}