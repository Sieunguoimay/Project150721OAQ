using Framework.Resolver;
using UnityEngine;

namespace Gameplay
{
    public abstract class MonoInjectable : MonoBehaviour, IInjectable
    {
        public virtual void Inject(IResolver resolver)
        {
        }
    }

    public abstract class MonoSelfBindingInjectable<TInjectable> : MonoInjectable, ISelfBindingInjectable
    {
        public void Bind(IBinder binder)
        {
            binder.Bind<TInjectable>(this);
        }

        public void Unbind(IBinder binder)
        {
            binder.Unbind<TInjectable>();
        }
    }
}