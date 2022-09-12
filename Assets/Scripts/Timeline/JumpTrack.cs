using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline
{
    [TrackClipType(typeof(JumpClip))]
    [TrackBindingType(typeof(Transform))]
    public class JumpTrack : TrackAsset
    {
        public string nothing;
        public TransformWithTargetMixerBehaviour template;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();

            var playable = ScriptPlayable<TransformWithTargetMixerBehaviour>.Create(graph, template, inputCount);
            playable.GetBehaviour().Setup(director.GetGenericBinding(this));

            return playable;
        }
    }

    [Serializable]
    public class TransformWithTargetMixerBehaviour : PlayableBehaviour
    {
        private Vector3 _initialPosition;

        public void Setup(UnityEngine.Object playerData)
        {
            _initialPosition = ((Transform) playerData).position;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var binding = playerData as Transform;
            if (binding == null) return;
            var inputCount = playable.GetInputCount();

            var initialWeight = 1f - Enumerable.Range(0, inputCount).Select(i => playable.GetInputWeight(i)).Sum();
            var finalPosition = _initialPosition;

            finalPosition.x = initialWeight * _initialPosition.x;
            finalPosition.z = initialWeight * _initialPosition.z;

            for (var i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                var clipPlayable = (ScriptPlayable<JumpBehaviour>) playable.GetInput(i);
                var inputBehaviour = clipPlayable.GetBehaviour();

                finalPosition.x += inputWeight * (inputBehaviour.xy.x + _initialPosition.x);
                finalPosition.z += inputWeight * (inputBehaviour.xy.y + _initialPosition.z);

                finalPosition.y +=  inputBehaviour.y + _initialPosition.y;
            }

            binding.position = finalPosition;
            Debug.Log(initialWeight);
        }
    }
}