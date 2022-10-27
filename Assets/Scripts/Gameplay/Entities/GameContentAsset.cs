using Framework.Entities;
using Framework.Entities.Currency;
using Framework.Services;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Entities
{
    [CreateAssetMenu]
    public class GameContentAsset : EntityAsset, IGameContentData
    {
        [field: SerializeField, IdSelector(typeof(ICurrencyData))]
        public string[] CurrencyIds { get; private set; }

        [field: SerializeField, IdSelector(typeof(ICurrencyProcessorData))]
        public string MatchProcessorId { get; private set; }

        public override IEntity<IEntityData, IEntitySavedData> CreateEntity()
        {
            return new GameContent(this, null);
        }
    }
}