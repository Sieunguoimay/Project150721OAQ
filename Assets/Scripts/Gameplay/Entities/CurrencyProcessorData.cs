using System;
using Framework.Entities;
using Framework.Entities.Currency;
using UnityEngine;

namespace Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Entity/CurrencyProcessorData")]

    public class CurrencyProcessorData : EntityAsset<ICurrencyProcessor>, ICurrencyProcessorData
    {
        [field: SerializeField] public CurrencyAmount[] Inputs { get; private set; }
        [field: SerializeField] public CurrencyAmount[] Outputs { get; private set; }

        protected override IEntity<IEntityData,IEntitySavedData> CreateEntityInternal()
        {
            return new CurrencyProcessor(this, null);
        }
    }
}