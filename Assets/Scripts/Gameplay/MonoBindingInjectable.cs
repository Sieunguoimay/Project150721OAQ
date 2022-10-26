using Framework.Resolver;
using UnityEngine;

namespace Gameplay
{
    public abstract class MonoBindingInjectable<TInjectable> : MonoBehaviour, IInjectable, IBinding
    {
        public void SelfBind(IBinder binder)
        {
            binder.Bind<TInjectable>(this);
        }

        public void SelfUnbind(IBinder binder)
        {
            binder.Unbind<TInjectable>(this);
        }

        public virtual void Inject(IResolver resolver)
        {
            
        }
    }
}