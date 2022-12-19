using Framework.Entities;
using UnityEngine;
using System;

namespace Gameplay.Entities.Stage.StageSelector
{
    public interface IStageSelectorData : IEntityData
    {
    }

    public interface IStageSelectorSavedData : IEntitySavedData
    {
    }

    [CreateAssetMenu(menuName = "Entity/StageSelectorData")]
    public class StageSelectorData : EntityAsset<IStageSelector>, IStageSelectorData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal()
        {
            return new StageSelector(this, new StageSelectorSavedData(Id));
        }
    }
    
    [Serializable]
    public class StageSelectorSavedData : BaseEntitySavedData, IStageSelectorSavedData
    {
        public StageSelectorSavedData(string id) : base(id)
        {
        }
    }
}