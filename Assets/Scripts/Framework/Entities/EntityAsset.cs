using System;
using Framework.Services.Data;

namespace Framework.Entities
{
    public abstract class EntityAsset<TEntity> : DataAsset, IEntityData
        where TEntity : class, IEntity<IEntityData, IEntitySavedData>
    {
        public IEntity<IEntityData, IEntitySavedData> CreateEntity(IEntityLoader entityLoader)
        {
            var entity = CreateEntityInternal(entityLoader);
#if UNITY_EDITOR
            DebugEntity = entity as TEntity;
#endif
            return entity;
        }

        public Type GetEntityType()
        {
            return typeof(TEntity);
        }

        protected abstract IEntity<IEntityData, IEntitySavedData> CreateEntityInternal(IEntityLoader entityLoader);
#if UNITY_EDITOR
        protected TEntity DebugEntity { get; private set; }
#endif
    }
}