using System;
using System.Collections.Generic;
using Common.UnityExtend.Attribute;
using Framework.Entities;
using Framework.Entities.Currency;
using Framework.Services.Data;
using Gameplay.Entities.Stage;
using UnityEngine;

namespace Gameplay.Entities.MagicFlower
{
    public interface IMagicFlowerUniqueData
    {
        int NumFlowers { get; }
        float ToBlossomDuration { get; }
        int PayoutAmountPerFlower { get; }
    }

    public interface IMagicFlowerUniqueSavedData
    {
        int CollectablePayoutAmount { get; }
        IReadOnlyList<double> BlossomTimeStamps { get; }
    }

    public interface IMagicFlowerData : IEntityData, IMagicFlowerUniqueData
    {
        string PayoutCurrencyId { get; }
    }

    public interface IMagicFlowerSavedData : IEntitySavedData, IMagicFlowerUniqueSavedData
    {
        void SetCollectablePayoutAmount(int count);
        void SetBlossomTimeStamp(int index, double timestamp);
        void CreateBlossomTimestampsArray(int count);
    }

    [CreateAssetMenu(menuName = "Entity/MagicFlowerData")]
    public class MagicFlowerData : EntityAsset<IMagicFlower>, IMagicFlowerData
    {
        protected override IEntity<IEntityData, IEntitySavedData> CreateEntityInternal(IEntityLoader entityLoader)
        {
            return new MagicFlower(this, new MagicFlowerSavedData(this));
        }

        [field: SerializeField] public int NumFlowers { get; private set; }
        [field: SerializeField] public float ToBlossomDuration { get; private set; }

        [field: SerializeField, DataAssetIdSelector(typeof(ICurrencyData))]
        public string PayoutCurrencyId { get; private set; }

        [field: SerializeField, Min(0)] public int PayoutAmountPerFlower { get; private set; }
        [SerializeField, ChildAsset] private StageData Test2;
    }

    [Serializable]
    public class MagicFlowerSavedData : BaseEntitySavedData<IMagicFlowerData>, IMagicFlowerSavedData
    {
        public MagicFlowerSavedData(IMagicFlowerData data) : base(data)
        {
        }

        [field: SerializeField] public int CollectablePayoutAmount { get; private set; }

        public IReadOnlyList<double> BlossomTimeStamps => blossomTimeStamps;

        [SerializeField] private double[] blossomTimeStamps;

        public void SetCollectablePayoutAmount(int count)
        {
            CollectablePayoutAmount = count;
            Save();
        }

        public void SetBlossomTimeStamp(int index, double timestamp)
        {
            blossomTimeStamps[index] = timestamp;
            Debug.Log($"SetBlossomTimeStamp {index} {timestamp}");
            Save();
        }

        public void CreateBlossomTimestampsArray(int count)
        {
            blossomTimeStamps = new double[count];
        }
    }
}