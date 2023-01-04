using Framework.Entities;
using Framework.Entities.ContainerEntity;
using UnityEngine;

namespace Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Entity/GameContentData")]
    public class GameContentData : ContainerEntityData<IGameContent>, IGameContentData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateContainerEntityInternal()
        {
            return new GameContent(this, null);
        }
    }
}