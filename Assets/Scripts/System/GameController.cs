using System.Collections.Generic;
using System.ResolveSystem;
using Gameplay;
using UnityEngine;

namespace System
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameObject[] prefabs;
        [SerializeField] private GameObject[] objects;

        private GameObject[] _spawnedParts;

        private readonly Container _container = new();

        private IInjectable[] _injectables;
        private IBinding[] _bindings;

        private void Start()
        {
            SpawnGame();
        }

        private void OnDestroy()
        {
            TearDown();
        }

        private void SpawnGame()
        {
            _spawnedParts = new GameObject[prefabs.Length];

            var allInjectables = new List<IInjectable>();
            var allBindings = new List<IBinding>();

            foreach (var t in objects)
            {
                allInjectables.AddRange(t.GetComponentsInChildren<IInjectable>(true));
                allBindings.AddRange(t.GetComponentsInChildren<IBinding>(true));
            }

            for (var i = 0; i < prefabs.Length; i++)
            {
                var pref = prefabs[i];
                _spawnedParts[i] = Instantiate(pref);
                allInjectables.AddRange(_spawnedParts[i].GetComponentsInChildren<IInjectable>(true));
                allBindings.AddRange(_spawnedParts[i].GetComponentsInChildren<IBinding>(true));
            }

            _injectables = allInjectables.ToArray();
            _bindings = allBindings.ToArray();

            foreach (var b in allBindings)
            {
                b.Bind(_container);
            }

            foreach (var injectable in _injectables)
            {
                injectable.Inject(_container);
            }
        }

        private void TearDown()
        {
            foreach (var b in _bindings)
            {
                b.Unbind(_container);
            }

            foreach (var spawned in _spawnedParts)
            {
                Destroy(spawned);
            }
        }
    }
}