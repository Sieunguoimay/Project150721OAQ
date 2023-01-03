using Framework.Entities;
using UnityEngine;
using System;
using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities.GamePlayer
{
    public interface IGamePlayerData : IContainerEntityData
    {
    }

    public interface IGamePlayerSavedData : IContainerEntitySavedData
    {
    }

    [CreateAssetMenu(menuName = "Entity/GamePlayerData")]
    public class GamePlayerData : ContainerEntityData<IGamePlayer>, IGamePlayerData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateContainerEntityInternal(IEntity<IEntityData, IEntitySavedData>[] components, IEntitySavedData[] savedDataItems)
        {
            return new GamePlayer(this, new GamePlayerSavedData(this, savedDataItems));
        }
    }

    [Serializable]
    public class GamePlayerSavedData : ContainerEntitySavedData<IGamePlayerData>, IGamePlayerSavedData
    {
        public GamePlayerSavedData(IGamePlayerData data, IEntitySavedData[] componentSavedDataItems) : base(data, componentSavedDataItems)
        {
        }
    }
}