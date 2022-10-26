using System;
using System.Linq;
using Common;
using Common.Curve.Mover;
using Gameplay.Piece.Activities;
using SNM;
using Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Gameplay.BambooStick
{
    public class BambooStickSpace : MonoBehaviour
    {
        public BezierTimelineClipMove mover;
        public PlayableDirector timeline;
        public PlayableDirector timeline2;
        public Transform start;
        public BambooStickPathPlan pathPlan;

        private Action _handler;

        public float GetMoverSpeed()
        {
            var cct = (timeline2.playableAsset as TimelineAsset)?.GetOutputTracks()
                .FirstOrDefault(t => t is CustomControlTrack);
            if (cct == null) return 1f;
            var clip = cct.GetClips().FirstOrDefault(t => t.asset is CustomTimeControlAsset);
            if (clip == null) return 1f;
            var clipLength = clip.end - clip.start;
            return mover.Distance / (float) clipLength;
        }

        public void StartTimelineMoving(Action handler)
        {
            _handler = handler;
            timeline.Play();
            timeline.stopped -= TimelineStopped;
            timeline.stopped += TimelineStopped;
        }

        private void TimelineStopped(PlayableDirector obj)
        {
            timeline.stopped -= TimelineStopped;
            _handler?.Invoke();
        }
    }
}