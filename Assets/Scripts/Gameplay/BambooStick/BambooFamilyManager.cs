using Common.ResolveSystem;
using Gameplay.Board;
using Gameplay.Board.BoardDrawing;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.BambooStick
{
    public class BambooFamilyManager : InjectableBehaviour<BambooFamilyManager>
    {
        [SerializeField] private BambooStickSpace[] bambooSticks;
        [SerializeField] private Transform[] bambooStickVisualTransforms;

        private BoardSketcher _boardSketcher;
        private BoardManager _boardManager;
        private int _timelineCount;

        public override void Setup(IResolver resolver)
        {
            base.Setup(resolver);
            _boardSketcher = resolver.Resolve<BoardSketcher>();
            _boardManager = resolver.Resolve<BoardManager>();
        }

        public void BeginAnimSequence()
        {
            _boardSketcher.Sketch(_boardManager.Board);
            _boardSketcher.SetPenBalls(bambooStickVisualTransforms);
            for (var i = 0; i < _boardSketcher.PenUsageNum; i++)
            {
                var stick = bambooSticks[i];
                stick.timeline.Play();
                stick.timeline.stopped += TimelineStopped;
                stick.end.position = _boardSketcher.Points[i * (_boardSketcher.Points.Count / _boardSketcher.PenUsageNum)];
            }
        }

        public void BeginDrawing()
        {
            _boardSketcher.StartDrawing();
        }

        private void TimelineStopped(PlayableDirector playableDirector)
        {
            _timelineCount++;
            if (_timelineCount == bambooSticks.Length)
            {
                BeginDrawing();
            }

            playableDirector.stopped -= TimelineStopped;
        }
    }
}