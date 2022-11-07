using System;
using System.Collections.Generic;
using Framework.Entities;
using UnityEngine;

namespace Gameplay.Entities.MagicFlower
{
    public interface IMagicFlowerUniqueData
    {
        int FlowerCount { get; }
        float ToBlossomDuration { get; }
    }

    public interface IMagicFlowerUniqueSavedData
    {
        int CollectableFlowerCount { get; }
        IReadOnlyList<double> BlossomTimeStamps { get; }
    }

    public interface IMagicFlowerData : IEntityData, IMagicFlowerUniqueData
    {
    }

    public interface IMagicFlowerSavedData : IEntitySavedData, IMagicFlowerUniqueSavedData
    {
        void SetCollectableFlowerCount(int count);
        void SetBlossomTimeStamp(int index, double timestamp);
        void CreateBlossomTimestampsArray(int count);
    }

    [CreateAssetMenu(menuName = "Entity/MagicFlowerData")]
    public class MagicFlowerData : EntityAsset<IMagicFlower>, IMagicFlowerData
    {
        public override IEntity<IEntityData, IEntitySavedData> CreateEntity()
        {
            return new MagicFlower(this, new MagicFlowerSavedData(Id));
        }

        [field: SerializeField] public int FlowerCount { get; private set; }
        [field: SerializeField] public float ToBlossomDuration { get; private set; }
    }

    [Serializable]
    public class MagicFlowerSavedData : BaseEntitySavedData, IMagicFlowerSavedData
    {
        public MagicFlowerSavedData(string id) : base(id)
        {
        }

        [field: SerializeField] public int CollectableFlowerCount { get; private set; }

        public IReadOnlyList<double> BlossomTimeStamps => blossomTimeStamps;

        [SerializeField] private double[] blossomTimeStamps;

        public void SetCollectableFlowerCount(int count)
        {
            CollectableFlowerCount = count;
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