using Framework.Entities;
using UnityEngine;
using System;
using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities.Stage
{
    public interface IStageData : IContainerEntityData
    {
        MatchData MatchData { get; }
    }

    public interface IStageSavedData : IContainerEntitySavedData
    {
    }

    public class StageData : ContainerEntityData<IStage>, IStageData
    {
        [field: SerializeField] public MatchData matchData;

        protected override IEntity<IEntityData, IEntitySavedData> CreateContainerEntityInternal()
        {
            return new Stage(this, new StageSavedData(this));
        }
        public MatchData MatchData => matchData;
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