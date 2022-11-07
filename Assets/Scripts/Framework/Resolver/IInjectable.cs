namespace Framework.Resolver
{
    public interface IInjectable
    {
        void Inject(IResolver resolver);
    }

    public interface ISelfBindingInjectable : IInjectable
    {
        void Bind(IBinder binder);
        void Unbind(IBinder binder);
    }
}