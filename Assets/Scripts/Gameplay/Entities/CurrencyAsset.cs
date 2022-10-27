using Framework.Entities;
using Framework.Entities.Currency;
using UnityEngine;

namespace Gameplay.Entities
{
    [CreateAssetMenu]
    public class CurrencyAsset : EntityAsset, ICurrencyData
    {
        [field: SerializeField] public double InitialAmount { get; private set; }

        public override IEntity<IEntityData, IEntitySavedData> CreateEntity()
        {
            return new Currency(this, null);
        }
    }
}