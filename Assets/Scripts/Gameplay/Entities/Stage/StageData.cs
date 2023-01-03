using Framework.Entities;
using UnityEngine;
using System;
using System.Linq;
using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities.Stage
{
    public interface IStageData : IContainerEntityData
    {
    }

    public interface IStageSavedData : IContainerEntitySavedData
    {
    }

    [CreateAssetMenu(menuName = "Entity/StageData")]
    public class StageData : ContainerEntityData<IStage>, IStageData
    {

        protected override IEntity<IEntityData, IEntitySavedData> CreateContainerEntityInternal(IEntity<IEntityData, IEntitySavedData>[] components, IEntitySavedData[] savedDataItems)
        {
            return new Stage(this, new StageSavedData(this, savedDataItems), components);
        }
    }

    [Serializable]
    public class StageSavedData : ContainerEntitySavedData<IStageData>, IStageSavedData
    {
        public StageSavedData(IStageData data, IEntitySavedData[] componentSavedDataItems) : base(data, componentSavedDataItems)
        {
        }
    }
}