using System.Collections.Generic;
using Framework;
using Framework.Resolver;
using Framework.Services;
using Gameplay;
using UnityEngine;

namespace System
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameObject[] prefabs;
        [SerializeField] private GameObject[] objects;

        private GameObject[] _spawnedParts;

        private IInjectable[] _injectables;
        private IBinding[] _bindings;

        private IBinder _binder;
        private IResolver _resolver;

        private void Start()
        {
            _resolver = MonoInstaller.Instance.Resolver;
            _binder = _resolver.Resolve<IBinder>();

            SpawnGame();
        }

        private void OnDestroy()
        {
            TearDown();
        }

        private void SpawnGame()
        {
            var allInjectables = new List<IInjectable>();
            var allBindings = new List<IBinding>();

            foreach (var t in objects)
            {
                allInjectables.AddRange(t.GetComponentsInChildren<IInjectable>(true));
                allBindings.AddRange(t.GetComponentsInChildren<IBinding>(true));
            }

            _spawnedParts = new GameObject[prefabs.Length];

            for (var i = 0; i < prefabs.Length; i++)
            {
                _spawnedParts[i] = Instantiate(prefabs[i]);
                allInjectables.AddRange(_spawnedParts[i].GetComponentsInChildren<IInjectable>(true));
                allBindings.AddRange(_spawnedParts[i].GetComponentsInChildren<IBinding>(true));
            }

            _injectables = allInjectables.ToArray();
            _bindings = allBindings.ToArray();

            foreach (var b in allBindings)
            {
                b.SelfBind(_binder);
            }

            foreach (var injectable in _injectables)
            {
                injectable.Inject(_resolver);
            }
        }

        private void TearDown()
        {
            foreach (var b in _bindings)
            {
                b.SelfUnbind(_binder);
            }

            foreach (var spawned in _spawnedParts)
            {
                Destroy(spawned);
            }
        }
    }
}