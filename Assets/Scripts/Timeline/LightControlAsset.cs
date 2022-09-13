using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Timeline
{
    public class LightControlAsset : PlayableAsset
    {
        [SerializeField] private LightControlBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
            => ScriptPlayable<LightControlBehaviour>.Create(graph, template);
    }

    [Serializable]
    public class LightControlBehaviour : PlayableBehaviour
    {
        public Color color = Color.white;
        public float intensity = 1f;
    }
}