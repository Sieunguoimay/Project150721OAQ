using System;
using Framework.Entities;

namespace Gameplay.Entities.Stage
{
    public interface IStage : IEntity<IStageData, IStageSavedData>
    {
        void SetToAvailable();
        event Action<EventArgs> AvailableSet;
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
            if (!SavedData.IsAvailable && Data.IsAvailableInAdvanced)
            {
                SavedData.SetAvailable(true);
            }
        }

        public void SetToAvailable()
        {
            if (SavedData.IsAvailable) return;
            SavedData.SetAvailable(true);
            AvailableSet?.Invoke(EventArgs.Empty);
        }

        public event Action<EventArgs> AvailableSet;

        public void Unlock()
        {
            if (!SavedData.IsAvailable || SavedData.IsUnlocked) return;
            SavedData.SetUnlock(true);
            Unlocked?.Invoke(this);
        }

        public event Action<IStage> Unlocked;
    }
}