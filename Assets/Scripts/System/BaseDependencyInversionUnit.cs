using Framework.Resolver;
using UnityEngine;

namespace System
{
    public abstract class BaseInjectableScriptableObject : ScriptableObject, IInjectable
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

    public abstract class BaseInjectable : MonoBehaviour, IInjectable
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

    public abstract class BaseDependencyInversionUnit : BaseInjectable, IDependencyInversionUnit
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

    public abstract class BaseDependencyInversionScriptableObject : BaseInjectableScriptableObject,
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

    public abstract class BaseGenericDependencyInversionUnit<TBindingType> : BaseDependencyInversionUnit
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

    public abstract class BaseGenericDependencyInversionScriptableObject<TBindingType>
        : BaseDependencyInversionScriptableObject
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