using System.Collections.Generic;
using Framework.Entities;
using UnityEngine;

namespace Gameplay.Entities.MagicFlower
{
    public class MagicFlowerEntityView : BaseEntityView<IMagicFlower, IMagicFlowerData>, IMagicFlowerUniqueData,
        IMagicFlowerUnique, IMagicFlowerUniqueSavedData
    {
        protected override void SafeStart()
        {
            base.SafeStart();
            EventTrigger?.Invoke(1);
        }

        public int FlowerCount => Entity.Data.FlowerCount;
        public float ToBlossomDuration => Entity.Data.ToBlossomDuration;

        public void GrantBlossom(int blossomIndex)
        {
            Entity.GrantBlossom(blossomIndex);
            EventTrigger?.Invoke(2);
        }

        public void Collect() => Entity.Collect();
        public int CollectableFlowerCount => Entity.SavedData.CollectableFlowerCount;
        public IReadOnlyList<double> BlossomTimeStamps => Entity.SavedData.BlossomTimeStamps;
    }
}