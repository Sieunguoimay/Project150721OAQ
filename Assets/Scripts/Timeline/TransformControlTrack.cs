using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline
{
    [TrackClipType(typeof(TransformControlClip))]
    [TrackBindingType(typeof(Transform))]
    public class TransformControlTrack : TrackAsset
    {
        public string label;
        public TransformControlMixerBehaviour template;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();

            var playable = ScriptPlayable<TransformControlMixerBehaviour>.Create(graph, template, inputCount);
            playable.GetBehaviour().Setup(director.GetGenericBinding(this));

            return playable;
        }
    }

    [Serializable]
    public class TransformControlMixerBehaviour : PlayableBehaviour
    {
        [SerializeField] private Options positionOptions;
        [SerializeField] private Options eulerAnglesOptions;
        [SerializeField] private bool local;


        private Vector3 _initialPosition;
        private Vector3 _initialEulerAngles;

        public void Setup(UnityEngine.Object playerData)
        {
            if (local)
            {
                _initialPosition = ((Transform) playerData).localPosition;
                _initialEulerAngles = ((Transform) playerData).localEulerAngles;
            }
            else
            {
                _initialPosition = ((Transform) playerData).position;
                _initialEulerAngles = ((Transform) playerData).eulerAngles;
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var binding = playerData as Transform;
            if (binding == null) return;
            var inputCount = playable.GetInputCount();

            var initialWeight = 1f - Enumerable.Range(0, inputCount).Select(i => playable.GetInputWeight(i)).Sum();
            var finalPosition = new Vector3
            {
                x = positionOptions.ignoreWeightX ? _initialPosition.x : initialWeight * _initialPosition.x,
                y = positionOptions.ignoreWeightY ? _initialPosition.y : initialWeight * _initialPosition.y,
                z = positionOptions.ignoreWeightZ ? _initialPosition.z : initialWeight * _initialPosition.z
            };
            var finalEulerAngles = new Vector3
            {
                x = eulerAnglesOptions.ignoreWeightX ? _initialEulerAngles.x : initialWeight * _initialEulerAngles.x,
                y = eulerAnglesOptions.ignoreWeightY ? _initialEulerAngles.y : initialWeight * _initialEulerAngles.y,
                z = eulerAnglesOptions.ignoreWeightZ ? _initialEulerAngles.z : initialWeight * _initialEulerAngles.z
            };
            for (var i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                var clipPlayable = (ScriptPlayable<TransformControlBehaviour>) playable.GetInput(i);
                var inputBehaviour = clipPlayable.GetBehaviour();

                var posX = positionOptions.relativeX
                    ? inputBehaviour.position.x + _initialPosition.x
                    : inputBehaviour.position.x;
                var posY = positionOptions.relativeY
                    ? inputBehaviour.position.y + _initialPosition.y
                    : inputBehaviour.position.y;
                var posZ = positionOptions.relativeZ
                    ? inputBehaviour.position.z + _initialPosition.z
                    : inputBehaviour.position.z;

                posX *= positionOptions.ignoreWeightX ? 1f : inputWeight;
                posY *= positionOptions.ignoreWeightY ? 1f : inputWeight;
                posZ *= positionOptions.ignoreWeightZ ? 1f : inputWeight;

                finalPosition.x += posX;
                finalPosition.y += posY;
                finalPosition.z += posZ;

                var eulerX = eulerAnglesOptions.relativeX
                    ? inputBehaviour.eulerAngles.x + _initialEulerAngles.x
                    : inputBehaviour.eulerAngles.x;
                var eulerY = eulerAnglesOptions.relativeY
                    ? inputBehaviour.eulerAngles.y + _initialEulerAngles.y
                    : inputBehaviour.eulerAngles.y;
                var eulerZ = eulerAnglesOptions.relativeZ
                    ? inputBehaviour.eulerAngles.z + _initialEulerAngles.z
                    : inputBehaviour.eulerAngles.z;

                eulerX *= eulerAnglesOptions.ignoreWeightX ? 1f : inputWeight;
                eulerY *= eulerAnglesOptions.ignoreWeightY ? 1f : inputWeight;
                eulerZ *= eulerAnglesOptions.ignoreWeightZ ? 1f : inputWeight;

                finalEulerAngles.x += eulerX;
                finalEulerAngles.y += eulerY;
                finalEulerAngles.z += eulerZ;
            }

            if (local)
            {
                binding.localPosition = finalPosition;
                binding.localEulerAngles = finalEulerAngles;
            }
            else
            {
                binding.position = finalPosition;
                binding.eulerAngles = finalEulerAngles;
            }
        }

        [Serializable]
        public class Options
        {
            public bool relativeX;
            public bool relativeY;
            public bool relativeZ;
            public bool ignoreWeightX;
            public bool ignoreWeightY;
            public bool ignoreWeightZ;
        }
    }
}