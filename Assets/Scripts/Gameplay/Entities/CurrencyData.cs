using System;
using Framework.Entities;
using Framework.Entities.Currency;
using UnityEngine;

namespace Gameplay.Entities
{
    [CreateAssetMenu]
    public class CurrencyData : EntityAsset, ICurrencyData
    {
        [field: SerializeField] public double InitialAmount { get; private set; }

        public override IEntity<IEntityData, IEntitySavedData> CreateEntity()
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