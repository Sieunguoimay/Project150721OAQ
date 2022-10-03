using Common.ResolveSystem;
using UnityEngine;

namespace Gameplay
{
    public interface IInjectable
    {
        void Bind(IResolver resolver);
        void Setup(IResolver resolver);
        void TearDown();
        void Unbind(IResolver resolver);
    }

    public abstract class InjectableBehaviour<TInjectable> : MonoBehaviour, IInjectable
    {
        public virtual void Bind(IResolver resolver)
        {
            resolver.Bind<TInjectable>(this);
        }

        public virtual void Setup(IResolver resolver)
        {
            
        }

        public virtual void TearDown()
        {
            
        }

        public virtual void Unbind(IResolver resolver)
        {
            resolver.Unbind<TInjectable>(this);
        }
    }
}