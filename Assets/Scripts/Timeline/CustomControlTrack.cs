using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline
{
    [TrackClipType(typeof(CustomTimeControlAsset), false)]
    public class CustomControlTrack : TrackAsset
    {
    }
}