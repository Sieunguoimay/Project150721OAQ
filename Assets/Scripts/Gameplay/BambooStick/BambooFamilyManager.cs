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
            for (var i = 0; i < _boardSketcher.PenUsageNum; i++)
            {
                var stick = bambooSticks[i];
                var index = i * (_boardSketcher.Points.Count / _boardSketcher.PenUsageNum);
                stick.timeline.Play();
                stick.timeline.stopped += TimelineStopped;
                var endPos = _boardSketcher.transform.TransformPoint(new Vector3(_boardSketcher.Points[index].x, 0,
                    _boardSketcher.Points[index].y));
                var forward = (_boardSketcher.Points[index] - _boardSketcher.Points[index + 1]).normalized;
                var endForward = _boardSketcher.transform.TransformPoint(new Vector3(forward.x, 0, forward.y));
                stick.pathPlan.PlanPath(stick.start.position, stick.start.forward, endPos, endForward);
            }
        }
        private void TimelineStopped(PlayableDirector playableDirector)
        {
            _timelineCount++;
            if (_timelineCount == _boardSketcher.PenUsageNum)
            {
                BeginDrawing();
                _timelineCount = 0;
            }

            playableDirector.stopped -= TimelineStopped;
        }
        public void BeginDrawing()
        {
            for (var i = 0; i < _boardSketcher.PenUsageNum; i++)
            {
                _boardSketcher.Pens[i].SetPenBall(bambooStickVisualTransforms[i]);
                _boardSketcher.Pens[i].Done += OnSketchingDone;
            }

            _boardSketcher.StartDrawing();
        }

        private void OnSketchingDone(VisualPen visualPen)
        {
            visualPen.Done -= OnSketchingDone;
            MoveSticksBackToTheForest();
        }

        public void MoveSticksBackToTheForest()
        {
            _timelineCount++;
            if (_timelineCount == _boardSketcher.PenUsageNum)
            {
                for (var i = 0; i < _boardSketcher.PenUsageNum; i++)
                {
                    var stick = bambooSticks[i];
                    var endPosition = stick.start.position;
                    var endForward = stick.start.forward;
                    stick.mover.ResetDisplacement();
                    stick.pathPlan.PlanPath(stick.mover.transform.position, stick.mover.transform.forward, endPosition, endForward);
                    stick.timeline.Play();
                }
            }
        }
    }
}