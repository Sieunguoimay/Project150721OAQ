using System;
using Framework.Services.Data;

namespace Framework.Entities
{
    public abstract class EntityAsset<TEntity> : DataAsset, IEntityData
        where TEntity : IEntity<IEntityData, IEntitySavedData>
    {
        public abstract IEntity<IEntityData, IEntitySavedData> CreateEntity();

        public Type GetBindingType()
        {
            return typeof(TEntity);
        }
    }
}