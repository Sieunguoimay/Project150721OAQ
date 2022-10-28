using Framework.Entities;
using Framework.Entities.Currency;
using UnityEngine;

namespace Gameplay.Entities
{
    [CreateAssetMenu]
    public class CurrencyProcessorData : EntityAsset, ICurrencyProcessorData
    {
        [field: SerializeField] public CurrencyAmount[] Inputs { get; private set; }
        [field: SerializeField] public CurrencyAmount[] Outputs { get; private set; }

        public override IEntity<IEntityData,IEntitySavedData> CreateEntity()
        {
            return new CurrencyProcessor(this, null);
        }
    }
}