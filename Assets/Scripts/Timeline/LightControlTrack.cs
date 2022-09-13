using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline
{
    [TrackClipType(typeof(LightControlAsset))]
    [TrackBindingType(typeof(Light))]
    public class LightControlTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<LightControlMixerBehaviour>.Create(graph, inputCount);
        }
    }
    
    public class LightControlMixerBehaviour : PlayableBehaviour
    {
        private Light _light = null;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_light == null) _light = playerData as Light;

            if (_light != null)
            {
                var finalIntensity = 0f;
                var finalColor = Color.black;
                var inputCount = playable.GetInputCount();
                for (var i = 0; i < inputCount; i++)
                {
                    var inputWeight = playable.GetInputWeight(i);
                    var inputPlayable = (ScriptPlayable<LightControlBehaviour>) playable.GetInput(i);
                    var input = inputPlayable.GetBehaviour();

                    finalIntensity += input.intensity * inputWeight;
                    finalColor += input.color * inputWeight;
                }

                _light.color = finalColor;
                _light.intensity = finalIntensity;
            }
        }
    }
}