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
}