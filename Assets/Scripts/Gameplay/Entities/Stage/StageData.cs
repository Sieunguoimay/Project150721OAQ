using Framework.Entities;
using UnityEngine;
using System;

namespace Gameplay.Entities.Stage
{
    public interface IStageData : IEntityData
    {
        bool IsAvailableInAdvanced { get; }
    }

    public interface IStageSavedData : IEntitySavedData
    {
        bool IsAvailable { get; }
        bool IsUnlocked { get; }
        void SetAvailable(bool state);
        void SetUnlock(bool state);
    }

    [CreateAssetMenu(menuName = "Entity/StageData")]
    public class StageData : EntityAsset<IStage>, IStageData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal()
        {
            return new Stage(this, new StageSavedData(Id));
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
    public class StageSavedData : BaseEntitySavedData, IStageSavedData
    {
        public StageSavedData(string id) : base(id)
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