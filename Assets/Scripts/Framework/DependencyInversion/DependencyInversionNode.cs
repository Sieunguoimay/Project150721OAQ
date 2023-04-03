using System.Collections.Generic;
using System.Linq;
using Framework.Resolver;

namespace Framework.DependencyInversion
{
    public interface IHierarchyNode
    {
        IHierarchyNode[] Children { get; }
    }

    public class DependencyInversionNode : SelfBindingDependencyInversionUnit, IHierarchyNode
    {
        public IHierarchyNode[] Children { get; private set; }

        private readonly DependencyInversionUnitContainer _container = new();

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            Children = _container.DependencyInversionUnitChildren.OfType<IHierarchyNode>().ToArray();
            _container.OnBind(binder);
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

    public class DependencyInversionUnitContainer
    {
        public List<IDependencyInversionUnit> DependencyInversionUnitChildren { get; } = new();

        public void OnBind(IBinder binder)
        {
            foreach (var dependencyInversionNode in DependencyInversionUnitChildren)
            {
                dependencyInversionNode.Bind(binder);
            }
        }

        public void OnUnbind(IBinder binder)
        {
            foreach (var dependencyInversionNode in DependencyInversionUnitChildren)
            {
                dependencyInversionNode.Unbind(binder);
            }
        }

        public void OnInject(IResolver resolver)
        {
            foreach (var dependencyInversionNode in DependencyInversionUnitChildren)
            {
                dependencyInversionNode.Inject(resolver);
            }
        }

        public void OnSetupDependencies()
        {
            foreach (var dependencyInversionNode in DependencyInversionUnitChildren)
            {
                dependencyInversionNode.SetupDependencies();
            }
        }

        public void OnTearDownDependencies()
        {
            foreach (var dependencyInversionNode in DependencyInversionUnitChildren)
            {
                dependencyInversionNode.TearDownDependencies();
            }
        }

        public void AddChildDependencyInversionUnit(IDependencyInversionUnit unit)
        {
            DependencyInversionUnitChildren.Add(unit);
        }
    }
}