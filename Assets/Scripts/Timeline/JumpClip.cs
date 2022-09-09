using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Timeline
{
    public class JumpClip : PlayableAsset
    {
        [SerializeField] private JumpBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<JumpBehaviour>.Create(graph, template);
            return playable;
        }
    }

    [Serializable]
    public class JumpBehaviour : PlayableBehaviour
    {
        public Vector2 xy;
        public float y;
    }
}