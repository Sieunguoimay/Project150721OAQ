using System;
using Framework.Entities;

namespace Gameplay.Entities.Stage
{
    public interface IStage : IEntity<IStageData, IStageSavedData>
    {
        void Unlock();
        event Action<IStage> Unlocked;
    }

    public class Stage : BaseEntity<IStageData, IStageSavedData>, IStage
    {
        public Stage(IStageData data, IStageSavedData savedData) : base(data, savedData)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            SetupPreData();
        }

        private void SetupPreData()
        {
            if (!SavedData.IsUnlocked && Data.IsPreUnlocked)
            {
                SavedData.SetUnlock(true);
            }
        }
        public void Unlock()
        {
            if (SavedData.IsUnlocked) return;
            SavedData.SetUnlock(true);
            Unlocked?.Invoke(this);
        }

        public event Action<IStage> Unlocked;
    }
}