using Framework.Entities;
using UnityEngine;
using System;
using System.Linq;
using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities.Stage
{
    public interface IStageData : IContainerEntityData
    {
        bool IsAvailableInAdvanced { get; }
    }

    public interface IStageSavedData : IContainerEntitySavedData
    {
        bool IsAvailable { get; }
        bool IsUnlocked { get; }
        void SetAvailable(bool state);
        void SetUnlock(bool state);
    }

    [CreateAssetMenu(menuName = "Entity/StageData")]
    public class StageData : ContainerEntityData<IStage>, IStageData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal()
        {
            var items = GetEntityItems();
            var savedDataItems = new StageSavedData(Id, items.Select(i => i.SavedData).ToArray());
            return new Stage(this, savedDataItems, items);
        }

        [field: SerializeField] public bool IsAvailableInAdvanced { get; private set; }

#if UNITY_EDITOR
        [ContextMenu(nameof(SetToAvailable))]
        private void SetToAvailable()
        {
            DebugEntity?.SetToAvailable();
        }

        [ContextMenu(nameof(Unlock))]
        private void Unlock()
        {
            DebugEntity?.Unlock();
        }
#endif
    }

    [Serializable]
    public class StageSavedData : ContainerEntitySavedData, IStageSavedData
    {
        public StageSavedData(string id, IEntitySavedData[] componentSavedDataItems) : base(id, componentSavedDataItems)
        {
        }

        [field: SerializeField] public bool IsAvailable { get; private set; }
        [field: SerializeField] public bool IsUnlocked { get; private set; }

        public void SetAvailable(bool state)
        {
            IsAvailable = state;
            Save();
        }

        public void SetUnlock(bool state)
        {
            IsUnlocked = state;
            Save();
        }
    }
}