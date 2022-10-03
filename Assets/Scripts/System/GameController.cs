using System.Collections.Generic;
using Common.ResolveSystem;
using Gameplay;
using UnityEngine;

namespace System
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameObject[] prefabs;
        [SerializeField] private GameObject[] objects;

        private GameObject[] _spawnedParts;

        private readonly Resolver _resolver = new();

        private IInjectable[] _injectables;

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
            
            foreach (var t in objects)
            {
                allInjectables.AddRange(t.GetComponentsInChildren<IInjectable>(true));
            }

            for (var i = 0; i < prefabs.Length; i++)
            {
                var pref = prefabs[i];
                _spawnedParts[i] = Instantiate(pref);
                allInjectables.AddRange(_spawnedParts[i].GetComponentsInChildren<IInjectable>(true));
            }

            _injectables = allInjectables.ToArray();

            foreach (var injectable in _injectables)
            {
                injectable.Bind(_resolver);
            }

            foreach (var injectable in _injectables)
            {
                injectable.Setup(_resolver);
            }
        }

        private void TearDown()
        {
            foreach (var injectable in _injectables)
            {
                injectable.TearDown();
            }

            foreach (var injectable in _injectables)
            {
                injectable.Unbind(_resolver);
            }

            foreach (var spawned in _spawnedParts)
            {
                Destroy(spawned);
            }
        }
    }
}