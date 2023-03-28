using Framework.Resolver;
using UnityEngine;

namespace System
{
    public abstract class InjectableScriptableObject : ScriptableObject, IInjectable
    {
        protected IResolver Resolver { get; private set; }

        public void Inject(IResolver resolver)
        {
            Resolver = resolver;
            OnInject(resolver);
        }

        protected virtual void OnInject(IResolver resolver)
        {
        }
    }

    public abstract class InjectableMonoBehaviour : MonoBehaviour, IInjectable
    {
        protected IResolver Resolver { get; private set; }

        public void Inject(IResolver resolver)
        {
            Resolver = resolver;
            OnInject(resolver);
        }

        protected virtual void OnInject(IResolver resolver)
        {
        }
    }

    public interface IDependencyInversionUnit : IInjectable
    {
        void Bind(IBinder binder);
        void SetupDependencies();
        void TearDownDependencies();
        void Unbind(IBinder binder);
    }

    public abstract class DependencyInversionMonoBehaviour : InjectableMonoBehaviour, IDependencyInversionUnit
    {
        public void Bind(IBinder binder)
        {
            OnBind(binder);
        }

        public void SetupDependencies()
        {
            OnSetupDependencies();
        }

        public void TearDownDependencies()
        {
            OnTearDownDependencies();
        }

        public void Unbind(IBinder binder)
        {
            OnUnbind(binder);
        }

        protected virtual void OnBind(IBinder binder)
        {
        }

        protected virtual void OnSetupDependencies()
        {
        }

        protected virtual void OnTearDownDependencies()
        {
        }

        protected virtual void OnUnbind(IBinder binder)
        {
        }
    }

    public abstract class DependencyInversionScriptableObject : InjectableScriptableObject,
        IDependencyInversionUnit
    {
        public void Bind(IBinder binder)
        {
            OnBind(binder);
        }

        public void SetupDependencies()
        {
            OnSetupDependencies();
        }

        public void TearDownDependencies()
        {
            OnTearDownDependencies();
        }

        public void Unbind(IBinder binder)
        {
            OnUnbind(binder);
        }

        protected virtual void OnBind(IBinder binder)
        {
        }

        protected virtual void OnSetupDependencies()
        {
        }

        protected virtual void OnTearDownDependencies()
        {
        }

        protected virtual void OnUnbind(IBinder binder)
        {
        }
    }

    public abstract class SelfBindingDependencyInversionMonoBehaviour : DependencyInversionMonoBehaviour
    {
        protected override void OnBind(IBinder binder)
        {
            binder.Bind(GetBindingType(), this);
            base.OnBind(binder);
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            binder.Unbind(GetBindingType());
        }

        protected virtual Type GetBindingType() => GetType();
    }

    public abstract class SelfBindingDependencyInversionScriptableObject
        : DependencyInversionScriptableObject
    {
        protected override void OnBind(IBinder binder)
        {
            binder.Bind(GetBindingType(), this);
            base.OnBind(binder);
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            binder.Unbind(GetBindingType());
        }

        protected virtual Type GetBindingType() => GetType();
    }
}