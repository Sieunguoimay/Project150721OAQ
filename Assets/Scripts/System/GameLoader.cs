using Framework;
using Framework.DependencyInversion;
using Framework.Resolver;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace System
{
    public class GameLoader : MonoBehaviour
    {
        //[SerializeField] private ScriptableEntity controller;
        [SerializeField] private AssetReference controllerReference;

        private IBinder _binder;
        private IResolver _resolver;

        public ScriptableEntity Controller => (ScriptableEntity)controllerReference.Asset;//?? controller;

        private IEnumerator Start()
        {

            yield return controllerReference.LoadAssetAsync<ScriptableEntity>();

            _resolver = EntityController.Instance.Resolver;
            _binder = _resolver.Resolve<IBinder>();

            Controller.Bind(_binder);
            Controller.Inject(_resolver);
            Controller.SetupDependencies();
        }

        private void OnDestroy()
        {
            Controller.TearDownDependencies();
            Controller.Unbind(_binder);
            controllerReference.ReleaseAsset();
        }
    }
}