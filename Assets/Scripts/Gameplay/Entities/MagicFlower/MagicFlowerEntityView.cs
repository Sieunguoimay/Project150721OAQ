using System.Collections.Generic;
using Common.Transaction;
using Framework;
using Framework.Entities;
using UnityEngine;

namespace Gameplay.Entities.MagicFlower
{
    public class MagicFlowerEntityView : BaseEntityView<IMagicFlower,IMagicFlowerData>, IMagicFlowerUniqueData,
        IMagicFlowerUnique, IMagicFlowerUniqueSavedData
    {
        public int NumFlowers => Entity.Data.NumFlowers;
        public float ToBlossomDuration => Entity.Data.ToBlossomDuration;
        public int PayoutAmountPerFlower => Entity.Data.PayoutAmountPerFlower;

        public void GrantBlossom(int blossomIndex)
        {
            Entity.GrantBlossom(blossomIndex);
        }

        public void Collect() => Entity.Collect();
        public int CollectablePayoutAmount => Entity.SavedData.CollectablePayoutAmount;
        public IReadOnlyList<double> BlossomTimeStamps => Entity.SavedData.BlossomTimeStamps;
        public int CollectableFlowerCount => Mathf.CeilToInt(Entity.SavedData.CollectablePayoutAmount / (float) Entity.Data.PayoutAmountPerFlower);
        public ITransaction[] CreateTransactions()
        {
            var transactions = new ITransaction[5];
            for (var i = 0; i < transactions.Length; i++)
            {
                transactions[i] = new BaseTransaction(new PayoutCurrencyTransactionResult(Entity));
            }

            return transactions;
        }

        private class PayoutCurrencyTransactionResult : InnerClass<IMagicFlower>, ITransactionResult
        {
            public void OnSend()
            {
                Context.SavedData.SetCollectablePayoutAmount(Context.SavedData.CollectablePayoutAmount - 1);
            }

            public void OnSuccess()
            {
                Context.PayoutCurrency.Add(1);
            }

            public void OnDiscard()
            {
                Context.SavedData.SetCollectablePayoutAmount(Context.SavedData.CollectablePayoutAmount + 1);
            }

            public PayoutCurrencyTransactionResult(IMagicFlower context) : base(context)
            {
            }
        }
    }
}