using System;
using System.Collections;
using Framework.Resolver;
using Framework.Services.Data;
using Gameplay;
using UnityEngine;

namespace Framework.Entities
{
    public class BaseEntityView<TEntity, TEntityData> : MonoInjectable, IEntityView<TEntity>
        where TEntity : IEntity<IEntityData, IEntitySavedData> where TEntityData : IEntityData
    {
#if UNITY_EDITOR
        public Type DataType => typeof(TEntityData);

        [DataAssetIdSelector(nameof(DataType))]
#endif
        [SerializeField]
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
    }
}