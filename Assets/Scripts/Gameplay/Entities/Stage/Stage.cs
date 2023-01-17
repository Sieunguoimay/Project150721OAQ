using System;
using Framework.Entities;
using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities.Stage
{
    public interface IStage : IContainerEntity<IStageData, IStageSavedData>
    {
        int Index { get; }
        void SetIndex(int index);
    }

    public class Stage : ContainerEntity<IStageData, IStageSavedData>, IStage
    {
        public Stage(IStageData data, IStageSavedData savedData) : base(data, savedData)
        {
        }

        public void SetIndex(int index) => Index = index;
        public int Index { get; private set; }
    }
}