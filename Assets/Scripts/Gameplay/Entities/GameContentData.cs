using System;
using Framework.Entities;
using Framework.Entities.Currency;
using Framework.Services;
using Framework.Services.Data;
using UnityEngine;

namespace Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Entity/GameContentData")]
    public class GameContentData : EntityAsset<IGameContent>, IGameContentData
    {
        [field: SerializeField, IdSelector(typeof(ICurrencyData))]
        public string[] CurrencyIds { get; private set; }

        [field: SerializeField, IdSelector(typeof(ICurrencyProcessorData))]
        public string MatchProcessorId { get; private set; }

        [field: SerializeField, IdSelector(typeof(IEntityData))]
        public string[] EntityIds { get; private set; }

        public override IEntity<IEntityData, IEntitySavedData> CreateEntity()
        {
            return new GameContent(this, null);
        }
    }
}