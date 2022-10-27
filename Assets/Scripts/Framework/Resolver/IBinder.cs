namespace Framework.Resolver
{
    public interface IBinder
    {
        void Bind<TType>(object target);
        void Bind<TType>(object target, string id);

        void Unbind<TType>(object target);
        void Unbind<TType>(object target, string id);
    }

    public interface IBinding
    {
        void SelfBind(IBinder binder);
        void SelfUnbind(IBinder binder);
    }
}