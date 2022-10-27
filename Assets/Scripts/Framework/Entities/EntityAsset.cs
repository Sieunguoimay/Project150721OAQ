using Framework.Services;
using UnityEngine;

namespace Framework.Entities
{
    public abstract class EntityAsset : DataAsset, IEntityData
    {
        public abstract IEntity<IEntityData, IEntitySavedData> CreateEntity();
    }
}