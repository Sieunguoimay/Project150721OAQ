using Common.ResolveSystem;

namespace Gameplay
{
    public interface IInjectable
    {
        void Inject(IResolver resolver);
    }
}