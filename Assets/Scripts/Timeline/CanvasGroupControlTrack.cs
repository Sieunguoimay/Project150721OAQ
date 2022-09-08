using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline
{
    [TrackClipType(typeof(CanvasGroupControlAsset))]
    [TrackBindingType(typeof(CanvasGroup))]
    public class CanvasGroupControlTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<CanvasGroupControlMixerBehaviour>.Create(graph, inputCount);
        }
    }

    public class CanvasGroupControlMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var binding = playerData as CanvasGroup;
            if (binding == null) return;
            var inputCount = playable.GetInputCount();

            var finalAlpha = 0f;
            var interactable = false;
            for (var i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<CanvasGroupControlBehaviour>) playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                if (inputWeight > 0f)
                {
                    interactable |= input.interactable;
                }
                finalAlpha += inputWeight * input.alpha;
            }

            binding.interactable = interactable;
            binding.alpha = finalAlpha;
        }
    }
}