using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Framework.Resolver;
using UnityEngine;

namespace Framework.DependencyInversion
{
    public class ScriptableEntity : SelfBindingDependencyInversionScriptableObject,
        IHierarchyNode
    {
        [SerializeField, TypeConstraint(typeof(IDependencyInversion))]
        private UnityEngine.Object[] children;
        public IHierarchyNode[] Children { get; private set; }

        private readonly DependencyInversionUnitContainer _container = new();
        public IEnumerable<UnityEngine.Object> SerializedChildren => children;

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            try
            {
                _container.DependencyInversionUnitChildren.AddRange(SerializedChildren.Select(sc => sc as IDependencyInversion));
                _container.OnBind(binder);
                Children = _container.DependencyInversionUnitChildren.OfType<IHierarchyNode>().ToArray();
            }catch (Exception e)
            {
                Debug.Log($"Err ({name}): {e.Message}");
            }
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            _container.OnUnbind(binder);
        }

        protected override void OnInject(IResolver resolver)
        {
            base.OnInject(resolver);
            _container.OnInject(resolver);
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _container.OnSetupDependencies();
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();
            _container.OnTearDownDependencies();
        }

        protected void AddChildDependencyInversionUnit(IDependencyInversion unit)
        {
            _container.AddChildDependencyInversionUnit(unit);
        }
    }
}