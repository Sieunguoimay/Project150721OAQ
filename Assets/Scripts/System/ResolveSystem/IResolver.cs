namespace Common.ResolveSystem
{
    public interface IResolver
    {
        void Bind<TType>(TType target);
        void Bind<TType>(object target);
        void Bind<TType>(TType target, string id);
        void Unbind<TType>(TType target);
        void Unbind<TType>(object target);
        void Unbind<TType>(TType target, string id);
        TType Resolve<TType>();
        TType Resolve<TType>(string id);
    }
}