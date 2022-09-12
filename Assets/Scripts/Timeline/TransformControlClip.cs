using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Timeline
{
    public class TransformControlClip : PlayableAsset
    {
        [SerializeField] private TransformControlBehaviour template;

        public TransformControlBehaviour Template => template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<TransformControlBehaviour>.Create(graph, template);
        }
    }

    [Serializable]
    public class TransformControlBehaviour : PlayableBehaviour
    {
        public Vector3 position;
        public Vector3 eulerAngles;
    }
}