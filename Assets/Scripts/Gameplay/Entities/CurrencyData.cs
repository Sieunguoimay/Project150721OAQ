using System;
using Framework.Entities;
using Framework.Entities.Currency;
using UnityEngine;

namespace Gameplay.Entities
{
    public class CurrencyData : EntityAsset<ICurrency>, ICurrencyData
    {
        [field: SerializeField] public double InitialAmount { get; private set; }

        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal(IEntityLoader entityLoader)
        {
            return new Currency(this, new CurrencySavedData(this));
        }
    }

    [Serializable]
    public class CurrencySavedData : BaseEntitySavedData<ICurrencyData>, ICurrencySavedData
    {
        [SerializeField] private double amount;


        public double Get()
        {
            return amount;
        }

        public void Set(double newValue)
        {
            amount = newValue;
            Save();
        }

        public CurrencySavedData(ICurrencyData data) : base(data)
        {
        }
    }
}