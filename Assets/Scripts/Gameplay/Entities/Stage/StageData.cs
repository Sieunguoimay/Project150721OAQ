using Framework.Entities;
using UnityEngine;
using System;
using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities.Stage
{
    public interface IStageData : IContainerEntityData
    {
        int PlayerNum { get; }
        int TilesPerGroup { get; }
        int NumCitizensInTile { get; }
        MatchData MatchData { get; }
    }

    public interface IStageSavedData : IContainerEntitySavedData
    {
    }

    [CreateAssetMenu(menuName = "Entity/StageData")]
    public class StageData : ContainerEntityData<IStage>, IStageData
    {
        [field: SerializeField] public MatchData matchData;

        protected override IEntity<IEntityData, IEntitySavedData> CreateContainerEntityInternal()
        {
            return new Stage(this, new StageSavedData(this));
        }

        [field: SerializeField, Min(2)] public int PlayerNum { get; private set; } = 2;
        [field: SerializeField, Min(3)] public int TilesPerGroup { get; private set; } = 5;
        [field: SerializeField, Min(1)] public int NumCitizensInTile { get; private set; } = 5;

        public MatchData MatchData => matchData;

#if UNITY_EDITOR
        [ContextMenu("UpdateMatchData")]
        private void UpdateMatchData()
        {
            matchData.playerNum = PlayerNum;
            matchData.tilesPerGroup = TilesPerGroup;
            matchData.numCitizensInTile = NumCitizensInTile;
        }
#endif
    }

    [Serializable]
    public class StageSavedData : ContainerEntitySavedData<IStageData>, IStageSavedData
    {
        public StageSavedData(IStageData data) : base(data)
        {
        }
    }

    [Serializable]
    public class MatchData
    {
        public int playerNum;
        public int tilesPerGroup;
        public int numCitizensInTile;
    }
}