using System;
using Framework.Entities;
using Framework.Resolver;

namespace Gameplay.Entities.Stage
{
    public class StageEntityView : BaseEntityView<IStage, IStageData>
    {
        protected override void SafeStart()
        {
            base.SafeStart();
            Entity.Unlocked -= OnStageUnlocked;
            Entity.Unlocked += OnStageUnlocked;
        }

        private void OnEnable()
        {
            if (Entity == null) return;
            Entity.Unlocked -= OnStageUnlocked;
            Entity.Unlocked += OnStageUnlocked;
        }

        private void OnDisable()
        {
            Entity.Unlocked -= OnStageUnlocked;
        }

        private void OnStageUnlocked(IStage obj)
        {
            EventTrigger?.Invoke(2);
        }

        public bool IsUnlocked => Entity.SavedData.IsUnlocked;
    }
}