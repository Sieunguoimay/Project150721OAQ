using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Resolver;
using UnityEngine;

namespace Framework.DependencyInversion
{
    public class RootScriptableEntity : ScriptableEntity
    {
        [SerializeField] private GameObject[] prefabs;

        private GameObject[] _spawnedGameObjects;
        private IDependencyInversion[] _dependencyInversionUnits;
        private IInjectable[] _injectables;

        protected override void OnBind(IBinder binder)
        {
            SpawnGameObjects();
            FindAllDependencyInversionUnits();
            FindInjectablesOnly();
            AddFoundDependencyInversionUnitsToChildren();
            base.OnBind(binder);
        }

        protected override void OnInject(IResolver resolver)
        {
            base.OnInject(resolver);
            InjectInjectablesOnly(resolver);
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            SetActiveAllSpawnedPrefabs();
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            DestroyGameObjects();
        }

        public void SpawnGameObjects()
        {
            _spawnedGameObjects = new GameObject[prefabs.Length];
            for (var i = 0; i < prefabs.Length; i++)
            {
                prefabs[i].SetActive(false);
                _spawnedGameObjects[i] = Instantiate(prefabs[i]);
            }
        }

        private void SetActiveAllSpawnedPrefabs()
        {
            foreach (var s in _spawnedGameObjects)
            {
                s.SetActive(true);
            }
        }
        public void DestroyGameObjects()
        {
            foreach (var spawned in _spawnedGameObjects)
            {
                Destroy(spawned);
            }
        }

        private void InjectInjectablesOnly(IResolver resolver)
        {
            foreach (var injectable in _injectables)
            {
                injectable.Inject(resolver);
            }
        }
 
        private void AddFoundDependencyInversionUnitsToChildren()
        {
            foreach (var b in _dependencyInversionUnits)
            {
                AddChildDependencyInversionUnit(b);
            }
        }
        private void FindAllDependencyInversionUnits()
        {
            _dependencyInversionUnits =
                GetInterfaceInChildrenOfSpawnedGameObjects<IDependencyInversion>().ToArray();
        }

        private void FindInjectablesOnly()
        {
            _injectables = GetInterfaceInChildrenOfSpawnedGameObjects<IInjectable>()
                .Where(i => !_dependencyInversionUnits.Contains(i)).ToArray();
        }

        private IEnumerable<TInterface> GetInterfaceInChildrenOfSpawnedGameObjects<TInterface>()
            where TInterface : class
        {
            return _spawnedGameObjects.SelectMany(t => t.GetComponentsInChildren<TInterface>(true));
        }
    }
}