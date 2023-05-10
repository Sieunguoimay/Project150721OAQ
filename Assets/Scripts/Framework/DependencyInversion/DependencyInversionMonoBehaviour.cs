using System;
using Framework.Resolver;

namespace Framework.DependencyInversion
{
    public abstract class DependencyInversionMonoBehaviour : InjectableMonoBehaviour, IDependencyInversion
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
}