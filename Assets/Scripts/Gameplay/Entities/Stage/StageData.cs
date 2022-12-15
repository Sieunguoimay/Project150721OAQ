using Framework.Entities;
using UnityEngine;
using System;

namespace Gameplay.Entities.Stage
{
    public interface IStageData : IEntityData
    {
        bool IsPreUnlocked { get; }
    }

    public interface IStageSavedData : IEntitySavedData
    {
        bool IsUnlocked { get; }
        void SetUnlock(bool state);
    }

    [CreateAssetMenu(menuName = "Entity/StageData")]
    public class StageData : EntityAsset<IStage>, IStageData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal()
        {
            return new Stage(this, new StageSavedData(Id));
        }

        [field: SerializeField] public bool IsPreUnlocked { get; private set; }

#if UNITY_EDITOR
        [ContextMenu("Unlock")]
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

        [field: SerializeField] public bool IsUnlocked { get; private set; }

        public void SetUnlock(bool state)
        {
            IsUnlocked = state;
            Save();
        }
    }
}