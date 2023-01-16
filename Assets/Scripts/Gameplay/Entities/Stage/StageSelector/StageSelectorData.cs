using Framework.Entities;
using UnityEngine;
using System;
using Framework.Entities.SimpleContainer;
using Framework.Services.Data;

namespace Gameplay.Entities.Stage.StageSelector
{
    public interface IStageSelectorData : IEntityData
    {
        string StageContainerId { get; }
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

        [field: SerializeField, DataAssetIdSelector(typeof(ISimpleContainerData))] public string StageContainerId { get; private set; }
    }

    [Serializable]
    public class StageSelectorSavedData : BaseEntitySavedData<IStageSelectorData>, IStageSelectorSavedData
    {
        public StageSelectorSavedData(IStageSelectorData data) : base(data)
        {
        }
    }
}