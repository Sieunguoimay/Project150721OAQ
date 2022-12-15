using Framework.Resolver;
using UnityEngine;

namespace Gameplay
{
    public abstract class MonoInjectable : MonoBehaviour, IInjectable
    {
        public virtual void Inject(IResolver resolver)
        {
        }

        public void Setup()
        {
            SetupInternal();
        }

        protected virtual void SetupInternal()
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