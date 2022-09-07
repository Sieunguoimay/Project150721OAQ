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
}