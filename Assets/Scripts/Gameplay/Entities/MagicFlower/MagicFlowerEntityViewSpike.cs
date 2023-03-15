using System;
using Framework.Services;
using UnityEngine;

namespace Gameplay.Entities.MagicFlower
{
    public class MagicFlowerEntityViewSpike : MagicFlowerEntityView
    {
        [SerializeField] private int blossomIndex;

        public float BlossomRemainingDuration =>
            (float) Math.Max(Entity.SavedData.BlossomTimeStamps[blossomIndex] - TimerService.GameTimeStampInSeconds, 0);

        public float ToBlossomDurationStep1 =>
            Mathf.Max(BlossomRemainingDuration - Entity.Data.ToBlossomDuration / 2, 0);

        public float ToBlossomDurationStep2 => Mathf.Max(BlossomRemainingDuration - ToBlossomDurationStep1, 0);

        public void GrantBlossom() => GrantBlossom(blossomIndex);
    }
}