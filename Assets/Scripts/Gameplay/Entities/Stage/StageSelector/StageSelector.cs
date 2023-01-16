using System;
using Framework.Entities;
using Framework.Entities.SimpleContainer;
using Framework.Resolver;
using UnityEngine;

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
        private ISimpleContainer _stageContainer;
        public StageSelector(IStageSelectorData data, IStageSelectorSavedData savedData) : base(data, savedData)
        {
        }

        public override void Inject(IResolver resolver)
        {
            base.Inject(resolver);
            _stageContainer = resolver.Resolve<ISimpleContainer>(Data.StageContainerId);
        }

        public void Select(IStage stage)
        {
            SelectedStage = stage;
            StageIndex = FindStageIndex(stage);
            Selected?.Invoke(EventArgs.Empty);
        }

        private int FindStageIndex(IStage stage)
        {
            if (stage == null) return -1;
            for (var i = 0; i < _stageContainer.Children.Count; i++)
            {
                if (_stageContainer.Children[i] == stage)
                {
                    return i;
                }
            }

            return -1;
        }

        public event Action<EventArgs> Selected;
        public IStage SelectedStage { get; private set; }
        public int StageIndex { get; private set; }
    }
}