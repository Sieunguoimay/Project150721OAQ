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
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal(IEntityLoader entityLoader)
        {
            return new StageSelector(this, new StageSelectorSavedData(this));
        }
    }
    
    [Serializable]
    public class StageSelectorSavedData : BaseEntitySavedData<IStageSelectorData>, IStageSelectorSavedData
    {
        public StageSelectorSavedData(IStageSelectorData data) : base(data)
        {
        }
    }
}