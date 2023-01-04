using System;
using Framework.Entities;
using Framework.Entities.ContainerEntity;
using UnityEngine;

namespace Gameplay.Entities.GamePlayRunner
{
    public interface IGamePlayRunnerData : IContainerEntityData
    {
    }

    public interface IGamePlayRunnerSavedData : IContainerEntitySavedData
    {
    }

    [CreateAssetMenu(menuName = "Entity/GamePlayRunnerData")]
    public class GamePlayRunnerData : ContainerEntityData<IGamePlayRunner>, IGamePlayRunnerData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateContainerEntityInternal()
        {
            return new GamePlayRunner(this, new GamePlayRunnerSavedData(this));
        }
    }

    [Serializable]
    public class GamePlayRunnerSavedData : ContainerEntitySavedData<IGamePlayRunnerData>, IGamePlayRunnerSavedData
    {
        public GamePlayRunnerSavedData(IGamePlayRunnerData data) : base(data)
        {
        }
    }
}