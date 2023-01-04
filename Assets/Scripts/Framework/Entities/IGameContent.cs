using Framework.Entities.ContainerEntity;
using Framework.Entities.Currency;
using Framework.Resolver;

namespace Framework.Entities
{
    public interface IGameContent : IContainerEntity<IGameContentData,IGameContentSavedData>
    {
        //Empty
    }

    public interface IGameContentData : IContainerEntityData
    {
    }

    public interface IGameContentSavedData : IContainerEntitySavedData
    {
    }
}