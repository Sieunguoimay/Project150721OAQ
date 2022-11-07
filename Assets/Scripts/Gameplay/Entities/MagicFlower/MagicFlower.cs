using Framework.Entities;
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
    }

    public class MagicFlower : BaseEntity<IMagicFlowerData, IMagicFlowerSavedData>, IMagicFlower
    {
        public MagicFlower(IMagicFlowerData data, IMagicFlowerSavedData savedData) : base(data, savedData)
        {
            if (savedData.BlossomTimeStamps == null || savedData.BlossomTimeStamps.Count == 0)
            {
                savedData.CreateBlossomTimestampsArray(data.FlowerCount);
            }
        }

        public void GrantBlossom(int blossomIndex)
        {
            SavedData.SetBlossomTimeStamp(blossomIndex, TimerService.GameTimeStampInSeconds + (long)Data.ToBlossomDuration);
            SavedData.SetCollectableFlowerCount(SavedData.CollectableFlowerCount + 1);
        }

        public void Collect()
        {
            if (SavedData.CollectableFlowerCount <= 0)
            {
                Debug.LogError("Not synchronized with view");
                return;
            }

            SavedData.SetCollectableFlowerCount(SavedData.CollectableFlowerCount - 1);
        }
    }
}