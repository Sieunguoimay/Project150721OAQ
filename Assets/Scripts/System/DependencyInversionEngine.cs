using Framework;
using Framework.DependencyInversion;
using Framework.Resolver;
using UnityEngine;

namespace System
{
    public class DependencyInversionEngine : MonoBehaviour
    {
        [SerializeField] private DependencyInversionScriptableObjectNode controller;

        private IBinder _binder;
        private IResolver _resolver;

        private void Start()
        {
            _resolver = EntityController.Instance.Resolver;
            _binder = _resolver.Resolve<IBinder>();

            controller.Bind(_binder);
            controller.Inject(_resolver);
            controller.SetupDependencies();
        }

        private void OnDestroy()
        {
            controller.TearDownDependencies();
            controller.Unbind(_binder);
        }
    }
}