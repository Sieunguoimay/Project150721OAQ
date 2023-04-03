using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Framework.Resolver;
using UnityEngine;

namespace Framework.DependencyInversion
{
    public class DependencyInversionScriptableObjectNode : SelfBindingDependencyInversionScriptableObject,
        IHierarchyNode
    {
        [SerializeField, TypeConstraint(typeof(IDependencyInversionUnit))]
        private UnityEngine.Object[] children;
        public IHierarchyNode[] Children { get; private set; }

        private readonly DependencyInversionUnitContainer _container = new();
        public IEnumerable<UnityEngine.Object> SerializedChildren => children;

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            _container.DependencyInversionUnitChildren.AddRange(SerializedChildren.Select(sc => sc as IDependencyInversionUnit));
            _container.OnBind(binder);
            Children = _container.DependencyInversionUnitChildren.OfType<IHierarchyNode>().ToArray();
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

        protected void AddChildDependencyInversionUnit(IDependencyInversionUnit unit)
        {
            _container.AddChildDependencyInversionUnit(unit);
        }
    }
}