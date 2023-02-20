using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.Resolver;
using Gameplay;
using UnityEngine;

namespace System
{
    public class ControlUnitManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] prefabs;

        private GameObject[] _spawnedGameObjects;
        private IControlUnit[] _controlUnits;

        private IBinder _binder;
        private IResolver _resolver;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => MonoInstaller.Instance.Done);

            _resolver = MonoInstaller.Instance.Resolver;
            _binder = _resolver.Resolve<IBinder>();

            Spawn();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Spawn()
        {
            var allControlUnits = new List<IControlUnit>();

            _spawnedGameObjects = new GameObject[prefabs.Length];

            for (var i = 0; i < prefabs.Length; i++)
            {
                prefabs[i].gameObject.SetActive(false);
                _spawnedGameObjects[i] = Instantiate(prefabs[i]);
                prefabs[i].gameObject.SetActive(true);
                allControlUnits.AddRange(_spawnedGameObjects[i].GetComponentsInChildren<IControlUnit>(true));
            }

            _controlUnits = allControlUnits.ToArray();

            foreach (var b in _controlUnits)
            {
                b.Bind(_binder);
            }

            foreach (var injectable in _controlUnits)
            {
                injectable.Inject(_resolver);
            }

            foreach (var injectable in _controlUnits)
            {
                injectable.Setup();
            }

            foreach (var s in _spawnedGameObjects)
            {
                s.gameObject.SetActive(true);
            }
        }

        private void Cleanup()
        {
            foreach (var b in _controlUnits)
            {
                b.TearDown();
            }
            
            foreach (var b in _controlUnits)
            {
                b.Unbind(_binder);
            }

            foreach (var spawned in _spawnedGameObjects)
            {
                Destroy(spawned);
            }
        }
    }
}