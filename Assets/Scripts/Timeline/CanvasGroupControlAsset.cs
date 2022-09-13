using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Timeline
{
    public class CanvasGroupControlAsset : PlayableAsset
    {
        [SerializeField] private CanvasGroupControlBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
            => ScriptPlayable<CanvasGroupControlBehaviour>.Create(graph, template);
    }

    [Serializable]
    public class CanvasGroupControlBehaviour : PlayableBehaviour
    {
        public float alpha = 1f;
        public bool interactable = true;
    }
}