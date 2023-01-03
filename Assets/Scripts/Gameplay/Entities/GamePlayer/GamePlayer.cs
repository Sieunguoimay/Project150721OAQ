using Framework.Entities;
using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities.GamePlayer
{
    public interface IGamePlayer : IContainerEntity<IGamePlayerData, IGamePlayerSavedData>
    {
    }

    public class GamePlayer : BaseEntity<IGamePlayerData, IGamePlayerSavedData>, IGamePlayer
    {
        public GamePlayer(IGamePlayerData data, IGamePlayerSavedData savedData) : base(data, savedData)
        {
        }

        public IEntity<IEntityData, IEntitySavedData>[] Components { get; }
    }
}