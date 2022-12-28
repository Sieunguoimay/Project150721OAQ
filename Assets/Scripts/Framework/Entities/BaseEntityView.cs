using System;
using System.Collections;
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
    public class BaseEntityView<TEntity, TEntityData> : MonoInjectable, IEntityView<TEntity>
        where TEntity : IEntity<IEntityData, IEntitySavedData> where TEntityData : IEntityData
    {
#if UNITY_EDITOR
        public IEnumerable<string> Ids => IdsHelper.GetIds(typeof(TEntityData));
#endif
        [SerializeField]
#if UNITY_EDITOR
        [StringSelector(nameof(Ids))]
#endif
        private string entityId;

        protected string EntityId => entityId;

        public TEntity Entity { get; private set; }

        public override void Inject(IResolver resolver)
        {
            Entity = resolver.Resolve<TEntity>(entityId);
            base.Inject(resolver);
        }

        protected override void SetupInternal()
        {
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => Entity != null);
            SafeStart();
        }

        protected virtual void SafeStart()
        {
        }
    }
}