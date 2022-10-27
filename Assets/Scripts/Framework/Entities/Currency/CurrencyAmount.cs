using System;
using Framework.Services;
using UnityEngine;

namespace Framework.Entities.Currency
{
    [Serializable]
    public struct CurrencyAmount
    {
        public CurrencyAmount(string currencyId, double amount)
        {
            CurrencyId = currencyId;
            Amount = amount;
        }

        [field: SerializeField, IdSelector(typeof(ICurrencyData))]
        public string CurrencyId { get; private set; }

        [field: SerializeField] public double Amount { get; private set; }
    }
}