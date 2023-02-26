using System;
using Framework.Entities;
using Framework.Entities.Currency;
using Framework.Resolver;
using Framework.Services;
using UnityEngine;

namespace Gameplay.Entities.MagicFlower
{
    public interface IMagicFlowerUnique
    {
        void GrantBlossom(int blossomIndex);
        void Collect();
    }

    public interface IMagicFlower : IEntity<IMagicFlowerData, IMagicFlowerSavedData>, IMagicFlowerUnique
    {
        ICurrency PayoutCurrency { get; }
        event Action<EventArgs> GrantedBlossom;
    }

    public class MagicFlower : BaseEntity<IMagicFlowerData, IMagicFlowerSavedData>, IMagicFlower
    {
        public MagicFlower(IMagicFlowerData data, IMagicFlowerSavedData savedData) : base(data, savedData)
        {
            if (savedData.BlossomTimeStamps == null || savedData.BlossomTimeStamps.Count == 0)
            {
                savedData.CreateBlossomTimestampsArray(data.NumFlowers);
            }
        }

        public override void SetupDependencies()
        {
            base.SetupDependencies();
            PayoutCurrency = Resolver.Resolve<ICurrency>(Data.PayoutCurrencyId);
        }

        public void GrantBlossom(int blossomIndex)
        {
            SavedData.SetBlossomTimeStamp(blossomIndex, TimerService.GameTimeStampInSeconds + (long) Data.ToBlossomDuration);
            SavedData.SetCollectablePayoutAmount(SavedData.CollectablePayoutAmount + Data.PayoutAmountPerFlower);
            GrantedBlossom?.Invoke(EventArgs.Empty);
        }

        public void Collect()
        {
            if (SavedData.CollectablePayoutAmount <= 0)
            {
                Debug.LogError("Not synchronized with view");
                return;
            }

            SavedData.SetCollectablePayoutAmount(Mathf.Max(SavedData.CollectablePayoutAmount - Data.PayoutAmountPerFlower, 0));
            PayoutCurrency.Add(Data.PayoutAmountPerFlower);
        }

        public event Action<EventArgs> GrantedBlossom;

        public ICurrency PayoutCurrency { get; private set; }
    }
}