using System.Collections;
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

        private MonoInjectable[] _monoInjectables;
        private ISelfBindingInjectable[] _bindings;

        private IBinder _binder;
        private IResolver _resolver;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => MonoInstaller.Instance.Done);

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
            var allInjectables = new List<MonoInjectable>();
            var allBindings = new List<ISelfBindingInjectable>();

            foreach (var t in objects)
            {
                allInjectables.AddRange(t.GetComponentsInChildren<MonoInjectable>(true));
                allBindings.AddRange(t.GetComponentsInChildren<ISelfBindingInjectable>(true));
            }

            _spawnedParts = new GameObject[prefabs.Length];

            for (var i = 0; i < prefabs.Length; i++)
            {
                prefabs[i].gameObject.SetActive(false);
                _spawnedParts[i] = Instantiate(prefabs[i]);
                prefabs[i].gameObject.SetActive(true);
                allInjectables.AddRange(_spawnedParts[i].GetComponentsInChildren<MonoInjectable>(true));
                allBindings.AddRange(_spawnedParts[i].GetComponentsInChildren<ISelfBindingInjectable>(true));
            }

            _monoInjectables = allInjectables.ToArray();
            _bindings = allBindings.ToArray();

            foreach (var b in allBindings)
            {
                b.Bind(_binder);
            }

            foreach (var injectable in _monoInjectables)
            {
                injectable.Inject(_resolver);
            }

            foreach (var injectable in _monoInjectables)
            {
                injectable.Setup();
            }

            foreach (var s in _spawnedParts)
            {
                s.gameObject.SetActive(true);
            }
        }

        private void TearDown()
        {
            foreach (var b in _bindings)
            {
                b.Unbind(_binder);
            }

            foreach (var spawned in _spawnedParts)
            {
                Destroy(spawned);
            }
        }
    }
}