namespace System.ResolveSystem
{
    public interface IInjectable
    {
        void Inject(IResolver resolver);
    }
}