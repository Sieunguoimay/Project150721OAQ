using Framework.Entities;
using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities
{
    public class GameContent : ContainerEntity<IGameContentData, IGameContentSavedData>, IGameContent
    {
        public GameContent(IGameContentData data, IGameContentSavedData savedData) : base(data, savedData)
        {
        }
    }
}