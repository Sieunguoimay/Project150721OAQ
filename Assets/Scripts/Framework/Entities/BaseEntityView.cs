﻿using System.Collections;
using System.Collections.Generic;
using Common.UnityExtend.Attribute;
using Common.UnityExtend.Reflection;
using Framework.Resolver;
using Framework.Services.Data;
using Gameplay;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Entities
{
    public class BaseEntityView<TEntity, TEntityData> : MonoInjectable, IDataBridgeTrigger
        where TEntity : IEntity<IEntityData, IEntitySavedData> where TEntityData : IEntityData
    {
        [SerializeField]
#if UNITY_EDITOR
        [StringSelector(nameof(Ids))]
#endif
        private string entityId;

        protected TEntity Entity { get; private set; }
        private bool _injected;

        public override void Inject(IResolver resolver)
        {
            Entity = resolver.Resolve<TEntity>(entityId);
            base.Inject(resolver);
            _injected = true;
        }
#if UNITY_EDITOR
        public IEnumerable<string> Ids => IdsHelper.GetIds(typeof(TEntityData));
#endif
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => _injected);
            SafeStart();
        }

        protected virtual void SafeStart()
        {
            
        }
        
        [field: System.NonSerialized] public UnityEvent<int> EventTrigger { get; private set; } = new();
        public IReadOnlyList<string> Filters => new[] {"static", "state", "progress"};
    }
}