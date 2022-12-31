using System;
using Framework.Entities;
using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities.Stage
{
    public interface IStage :  IContainerEntity<IStageData, IStageSavedData>
    {
    }

    public class Stage : ContainerEntity<IStageData, IStageSavedData>, IStage
    {
        public Stage(IStageData data, IStageSavedData savedData, IEntity<IEntityData, IEntitySavedData>[] components) :
            base(data, savedData, components)
        {
        }
    }
}