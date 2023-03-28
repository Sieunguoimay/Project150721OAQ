using System.Collections.Generic;
using System.Linq;
using Framework;
using Framework.Resolver;
using UnityEngine;

namespace System
{
    public class DependencyInversionController : DependencyInversionMonoBehaviourNode
    {
        [SerializeField] private GameObject[] prefabs;

        private GameObject[] _spawnedGameObjects;
        private IDependencyInversionUnit[] _dependencyInversionUnits;
        private IInjectable[] _injectables;
        private IBinder _binder;
        private IResolver _resolver;

        private void Start()
        {
            _resolver = EntityController.Instance.Resolver;
            _binder = _resolver.Resolve<IBinder>();

            SpawnGameObjects();
            FindAllDependencyInversionUnits();
            FindAllInjectables();
            RootSetupDependencies();
        }

        private void OnDestroy()
        {
            RootTearDownDependencies();
            DestroyGameObjects();
        }

        private void SpawnGameObjects()
        {
            _spawnedGameObjects = new GameObject[prefabs.Length];
            for (var i = 0; i < prefabs.Length; i++)
            {
                prefabs[i].gameObject.SetActive(false);
                _spawnedGameObjects[i] = Instantiate(prefabs[i]);
            }
        }

        private void FindAllDependencyInversionUnits()
        {
            _dependencyInversionUnits = DependencyInversionUnitChildren
                .Concat(GetInterfaceInChildrenOfSpawnedGameObjects<IDependencyInversionUnit>())
                .ToArray();
        }

        private void FindAllInjectables()
        {
            _injectables = DependencyInversionUnitChildren
                .Concat(GetInterfaceInChildrenOfSpawnedGameObjects<IInjectable>())
                .ToArray();
        }

        private IEnumerable<TInterface> GetInterfaceInChildrenOfSpawnedGameObjects<TInterface>()
            where TInterface : class
        {
            return _spawnedGameObjects.SelectMany(t => t.GetComponentsInChildren<TInterface>(true));
        }

        private void RootSetupDependencies()
        {
            foreach (var b in _dependencyInversionUnits)
            {
                b.Bind(_binder);
            }

            foreach (var injectable in _injectables)
            {
                injectable.Inject(_resolver);
            }

            foreach (var injectable in _dependencyInversionUnits)
            {
                injectable.SetupDependencies();
            }

            foreach (var s in _spawnedGameObjects)
            {
                s.gameObject.SetActive(true);
            }
        }

        private void RootTearDownDependencies()
        {
            foreach (var b in _dependencyInversionUnits)
            {
                b.TearDownDependencies();
            }

            foreach (var b in _dependencyInversionUnits)
            {
                b.Unbind(_binder);
            }
        }

        private void DestroyGameObjects()
        {
            foreach (var spawned in _spawnedGameObjects)
            {
                Destroy(spawned);
            }
        }
    }
}