﻿using Framework.Entities;
using Framework.Entities.Currency;
using Framework.Resolver;
using Framework.Services;

namespace Gameplay.Entities
{
    public class GameContent : Entity<IGameContentData, IGameContentSavedData>, IGameContent
    {
        private IEntityLoader _entityLoader;

        public GameContent(IGameContentData data, IGameContentSavedData savedData) : base(data, savedData)
        {
        }

        public override void Inject(IResolver resolver)
        {
            _entityLoader = resolver.Resolve<IEntityLoader>();

            foreach (var currencyId in Data.CurrencyIds)
            {
                _entityLoader.CreateEntity<ICurrency, ICurrencyData>(currencyId);
            }

            _entityLoader.CreateEntity<ICurrencyProcessor, ICurrencyProcessorData>(Data.MatchProcessorId);
        }

        public override void Terminate()
        {
            foreach (var currencyId in Data.CurrencyIds)
            {
                _entityLoader.DestroyEntity<ICurrency>(currencyId);
            }

            _entityLoader.DestroyEntity<ICurrencyProcessor>(Data.MatchProcessorId);
        }
    }
}