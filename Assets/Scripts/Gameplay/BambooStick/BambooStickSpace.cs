using System;
using System.Linq;
using Common.Curve.Mover;
using Common.Tools.MeshSaver;
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
        public GameObject activeOnStop;
        public MultipleMeshProvider activeOnMove;

        private Action _handler;

        private void Start()
        {
            foreach (var r in activeOnMove.Renderers)
            {
                r.gameObject.SetActive(false);
            }

            activeOnStop.SetActive(true);
        }

        public float GetMoverSpeed()
        {
            var cct = (timeline2.playableAsset as TimelineAsset)?.GetOutputTracks().FirstOrDefault(t => t is CustomControlTrack);
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
            foreach (var r in activeOnMove.Renderers)
            {
                r.gameObject.SetActive(true);
            }

            activeOnStop.SetActive(false);
        }

        public void ResetState()
        {
            timeline.Stop();
        }

        private void TimelineStopped(PlayableDirector obj)
        {
            timeline.stopped -= TimelineStopped;
            _handler?.Invoke();
            foreach (var r in activeOnMove.Renderers)
            {
                r.gameObject.SetActive(false);
            }

            activeOnStop.SetActive(true);
        }
    }
}