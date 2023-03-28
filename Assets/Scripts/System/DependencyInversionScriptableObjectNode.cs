using System.Linq;
using Common.UnityExtend.Attribute;
using Framework.Resolver;
using UnityEngine;

namespace System
{
    public class DependencyInversionScriptableObjectNode : SelfBindingDependencyInversionScriptableObject,
        IHierarchyNode
    {
        [SerializeField, TypeConstraint(typeof(IDependencyInversionUnit))]
        private UnityEngine.Object[] children;

        public IHierarchyNode[] Children { get; private set; }
        public IDependencyInversionUnit[] DependencyInversionUnitChildren { get; private set; }
        public UnityEngine.Object[] SerializedChildren => children;

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            Children = SerializedChildren.Select(sc => sc as IHierarchyNode).ToArray();
            DependencyInversionUnitChildren = SerializedChildren.Select(sc => sc as IDependencyInversionUnit).ToArray();
            foreach (var dependencyInversionNode in DependencyInversionUnitChildren)
            {
                dependencyInversionNode.Bind(binder);
            }
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            foreach (var dependencyInversionNode in DependencyInversionUnitChildren)
            {
                dependencyInversionNode.Unbind(binder);
            }
        }

        protected override void OnInject(IResolver resolver)
        {
            base.OnInject(resolver);
            foreach (var dependencyInversionNode in DependencyInversionUnitChildren)
            {
                dependencyInversionNode.Inject(resolver);
            }
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();

            foreach (var dependencyInversionNode in DependencyInversionUnitChildren)
            {
                dependencyInversionNode.SetupDependencies();
            }
        }

        protected override void OnTearDownDependencies()
        {
            base.OnTearDownDependencies();

            foreach (var dependencyInversionNode in DependencyInversionUnitChildren)
            {
                dependencyInversionNode.TearDownDependencies();
            }
        }
    }
}