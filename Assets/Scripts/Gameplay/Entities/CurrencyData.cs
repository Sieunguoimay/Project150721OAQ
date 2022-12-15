using System;
using Framework.Entities;
using Framework.Entities.Currency;
using UnityEngine;

namespace Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Entity/CurrencyData")]
    public class CurrencyData : EntityAsset<ICurrency>, ICurrencyData
    {
        [field: SerializeField] public double InitialAmount { get; private set; }

        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal()
        {
            return new Currency(this, new CurrencySavedData(Id));
        }
    }

    [Serializable]
    public class CurrencySavedData : BaseEntitySavedData, ICurrencySavedData
    {
        [SerializeField] private double amount;

        public CurrencySavedData(string id) : base(id)
        {
        }

        public double Get()
        {
            return amount;
        }

        public void Set(double newValue)
        {
            amount = newValue;
            Save();
        }
    }
}