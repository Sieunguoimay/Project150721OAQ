using Framework.Resolver;
using UnityEngine;

namespace Gameplay
{
    public interface IControlUnit : IInjectable
    {
        void Bind(IBinder binder);
        void Setup();
        void TearDown();
        void Unbind(IBinder binder);
    }

    public abstract class MonoControlUnitBase : MonoBehaviour, IControlUnit
    {
        protected IResolver Resolver { get; private set; }
        public void Bind(IBinder binder)
        {
            OnBind(binder);
        }

        public void Inject(IResolver resolver)
        {
            Resolver = resolver;
            OnInject(resolver);
        }

        public void Setup()
        {
            OnSetup();
        }

        public void TearDown()
        {
            OnTearDown();
        }

        public void Unbind(IBinder binder)
        {
            OnUnbind(binder);
        }

        protected virtual void OnBind(IBinder binder)
        {
        }

        protected virtual void OnInject(IResolver resolver)
        {
        }

        protected virtual void OnSetup()
        {
        }

        protected virtual void OnTearDown()
        {
        }

        protected virtual void OnUnbind(IBinder binder)
        {
        }
    }

    public abstract class MonoControlUnitBase<TBindingType> : MonoControlUnitBase
    {
        protected override void OnBind(IBinder binder)
        {
            binder.Bind<TBindingType>(this);
            base.OnBind(binder);
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            binder.Unbind<TBindingType>();
        }
    }
}