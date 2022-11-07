using UnityEngine;

namespace Gameplay.Entities.MagicFlower
{
    public class MagicFlowerEntityViewSpike : MagicFlowerEntityView
    {
        [SerializeField] private int blossomIndex;

        public float BlossomRemainingDuration =>
            (float) (Entity.SavedData.BlossomTimeStamps[blossomIndex] - Time.time);

        public float ToBlossomDurationStep1 =>
            Mathf.Max(BlossomRemainingDuration / 2 - Entity.Data.ToBlossomDuration / 2, 0);

        public float ToBlossomDurationStep2 => BlossomRemainingDuration - ToBlossomDurationStep1;

        public void GrantBlossom() => GrantBlossom(blossomIndex);
    }
}