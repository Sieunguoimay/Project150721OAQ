using System;
using Framework.Entities;
using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities.Stage
{
    public interface IStage :  IContainerEntity<IStageData, IStageSavedData>
    {
        void SetToAvailable();
        event Action<EventArgs> AvailableSet;
        void Unlock();
        event Action<IStage> Unlocked;
    }

    public class Stage : ContainerEntity<IStageData, IStageSavedData>, IStage
    {
        public Stage(IStageData data, IStageSavedData savedData, IEntity<IEntityData, IEntitySavedData>[] components) :
            base(data, savedData, components)
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