using System;
using Framework.Entities;
using Framework.Entities.SimpleContainer;
using Framework.Resolver;

namespace Gameplay.Entities.Stage.StageSelector
{
    public interface IStageSelector : IEntity<IStageSelectorData, IStageSelectorSavedData>
    {
        void Select(IStage stage);
        event Action<EventArgs> Selected;
        IStage SelectedStage { get; }
        int StageIndex { get; }
    }

    public class StageSelector : BaseEntity<IStageSelectorData, IStageSelectorSavedData>, IStageSelector
    {
        public StageSelector(IStageSelectorData data, IStageSelectorSavedData savedData) : base(data, savedData)
        {
        }

        public void Select(IStage stage)
        {
            SelectedStage = stage;
            Selected?.Invoke(EventArgs.Empty);
        }

        public event Action<EventArgs> Selected;
        public IStage SelectedStage { get; private set; }
        public int StageIndex => SelectedStage?.Index ?? -1;
    }
}