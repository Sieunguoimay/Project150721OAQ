using System.Collections;
using Framework.Resolver;
using Framework.Services.Data;
using Gameplay;
using UnityEngine;

namespace Framework.Entities
{
    public class BaseEntityView<TEntity> : MonoInjectable, IEntityView<TEntity>
        where TEntity : IEntity<IEntityData, IEntitySavedData>
    {
        [SerializeField]
#if UNITY_EDITOR
        [DataAssetIdSelector(typeof(IEntityData))]
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
    }
}