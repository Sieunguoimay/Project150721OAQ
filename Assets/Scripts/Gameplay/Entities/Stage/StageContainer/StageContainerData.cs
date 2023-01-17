using System;
using Framework.Entities;
using Framework.Entities.ContainerEntity;
using UnityEngine;

namespace Gameplay.Entities.Stage.StageContainer
{
    public interface IStageContainerData : IContainerEntityData
    {
    }

    public interface IStageContainerSavedData : IContainerEntitySavedData
    {
    }

    [CreateAssetMenu(menuName = "Entity/StageContainerData")]
    public class StageContainerData : ContainerEntityData<IStageContainer>, IStageContainerData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateContainerEntityInternal()
        {
            return new StageContainer(this, new StageContainerSavedData(this));
        }
    }

    [Serializable]
    public class StageContainerSavedData : ContainerEntitySavedData<IStageContainerData>, IStageContainerSavedData
    {
        public StageContainerSavedData(IStageContainerData data) : base(data)
        {
        }
    }
}