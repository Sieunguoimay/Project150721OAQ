using System;
using Framework.Services;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Entities.MagicFlower
{
    public class MagicFlowerEntityViewSpike : MagicFlowerEntityView
    {
        [SerializeField] private int blossomIndex;
        [SerializeField] private UnityEvent shouldChangeState;

        protected override void SafeStart()
        {
            base.SafeStart();

            if (BlossomRemainingDuration > 0)
            {
                EventTrigger?.Invoke(2);
                shouldChangeState?.Invoke();
            }
        }

        public float BlossomRemainingDuration =>
            (float) Math.Max(Entity.SavedData.BlossomTimeStamps[blossomIndex] - TimerService.GameTimeStampInSeconds, 0);

        public float ToBlossomDurationStep1 =>
            Mathf.Max(BlossomRemainingDuration - Entity.Data.ToBlossomDuration / 2, 0);

        public float ToBlossomDurationStep2 => Mathf.Max(BlossomRemainingDuration - ToBlossomDurationStep1, 0);

        public void GrantBlossom() => GrantBlossom(blossomIndex);
    }
}