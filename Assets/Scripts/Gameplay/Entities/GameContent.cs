using Framework.Entities;
using Framework.Entities.Currency;
using Framework.Resolver;

namespace Gameplay.Entities
{
    public class GameContent : BaseEntity<IGameContentData, IGameContentSavedData>, IGameContent
    {
        private IEntityLoader _entityLoader;
        private IEntity<IEntityData, IEntitySavedData>[] _entities;

        public GameContent(IGameContentData data, IGameContentSavedData savedData) : base(data, savedData)
        {
        }

        public override void Inject(IResolver resolver)
        {
            base.Inject(resolver);
            _entityLoader = resolver.Resolve<IEntityLoader>();

            _entities = new IEntity<IEntityData, IEntitySavedData>[Data.EntityIds.Length];
            foreach (var currencyId in Data.CurrencyIds)
            {
                _entityLoader.CreateEntity(currencyId);
            }

            _entityLoader.CreateEntity(Data.MatchProcessorId);

            for (var i = 0; i < Data.EntityIds.Length; i++)
            {
                var entityId = Data.EntityIds[i];
                _entities[i] = _entityLoader.CreateEntity(entityId);
            }
        }

        public override void Terminate()
        {
            foreach (var currencyId in Data.CurrencyIds)
            {
                _entityLoader.DestroyEntity( Resolver.Resolve<ICurrency>(currencyId));
            }

            _entityLoader.DestroyEntity(Resolver.Resolve<ICurrencyProcessor>(Data.MatchProcessorId));

            for (var i = 0; i < Data.EntityIds.Length; i++)
            {
                _entityLoader.DestroyEntity(_entities[i]);
            }
        }
    }
}