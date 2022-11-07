using Framework.Resolver;
using UnityEngine;

namespace Gameplay
{
    public abstract class MonoInjectable<TInjectable> : MonoBehaviour, ISelfBindingInjectable
    {
        public void Bind(IBinder binder)
        {
            binder.Bind<TInjectable>(this);
        }

        public void Unbind(IBinder binder)
        {
            binder.Unbind<TInjectable>();
        }

        public virtual void Inject(IResolver resolver)
        {

        }
    }
}