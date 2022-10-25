namespace System.ResolveSystem
{
    public interface IBinder
    {
        void Bind<TType>(TType target);
        void Bind<TType>(object target);
        void Bind<TType>(TType target, string id);
        void Unbind<TType>(TType target);
        void Unbind<TType>(object target);
        void Unbind<TType>(TType target, string id);
    }
    
    public interface IBinding
    {
        void Bind(IBinder binder);
        void Unbind(IBinder binder);
    }
}