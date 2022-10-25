using System.ResolveSystem;
using UnityEngine;

namespace Gameplay
{
    public abstract class MonoBindingInjectable<TInjectable> : MonoBehaviour, IInjectable, IBinding
    {
        public void Bind(IBinder binder)
        {
            binder.Bind<TInjectable>(this);
        }

        public void Unbind(IBinder binder)
        {
            binder.Unbind<TInjectable>(this);
        }

        public virtual void Inject(IResolver resolver)
        {
            
        }
    }
}