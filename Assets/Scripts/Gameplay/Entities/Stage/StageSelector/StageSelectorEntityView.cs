using Framework.Entities;
using Framework.Resolver;
using UnityEngine;

namespace Gameplay.Entities.Stage.StageSelector
{
    public class StageSelectorEntityView : BaseEntityView<IStageSelector>
    {
        public bool AnyStageSelected => Entity.SelectedStage != null;

        public void Select(IStage stage)
        {
            Entity.Select(stage);
        }

        public void Unselect()
        {
            Entity.Select(null);
        }
    }
}