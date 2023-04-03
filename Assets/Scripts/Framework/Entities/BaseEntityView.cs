using System;
using System.Collections;
using Framework.DependencyInversion;
using Framework.Resolver;
using Framework.Services.Data;
using Gameplay;
using UnityEngine;

namespace Framework.Entities
{
    public class BaseEntityView<TEntity, TEntityData> : InjectableMonoBehaviour, IEntityView<TEntity>
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

        protected override void OnInject(IResolver resolver)
        {
            Entity = resolver.Resolve<TEntity>(entityId);
        }
    }
}