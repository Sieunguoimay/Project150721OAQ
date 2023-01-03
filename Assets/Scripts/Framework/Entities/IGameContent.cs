using Framework.Entities.Currency;
using Framework.Resolver;

namespace Framework.Entities
{
    public interface IGameContent : IEntity<IGameContentData,IGameContentSavedData>
    {
        //Empty
    }

    public interface IGameContentData : IEntityData
    {
        string[] CurrencyIds { get; }
        string[] EntityIds { get; }
    }

    public interface IGameContentSavedData : IEntitySavedData
    {
        
    }
}