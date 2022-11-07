using Common.Curve.Mover;
using Framework;
using Framework.Resolver;
using Gameplay.Board;
using Gameplay.Board.BoardDrawing;
using SNM;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.BambooStick
{
    public class BambooFamilyManager : MonoSelfBindingInjectable<BambooFamilyManager>
    {
        [SerializeField] private BambooStickSpace[] bambooSticks;
        [SerializeField] private Transform[] bambooStickVisualTransforms;

        private BoardSketcher _boardSketcher;
        private BoardManager _boardManager;
        private int _timelineCount;

        public override void Inject(IResolver resolver)
        {
            _boardSketcher = resolver.Resolve<BoardSketcher>();
            _boardManager = resolver.Resolve<BoardManager>();
        }

        public void BeginAnimSequence()
        {
            _boardSketcher.Sketch(_boardManager.Board);
            for (var i = 0; i < _boardSketcher.PenUsageNum; i++)
            {
                var stick = bambooSticks[i];
                var startDrawingIndex = i * (_boardSketcher.Points.Count / _boardSketcher.PenUsageNum);
                stick.StartTimelineMoving(TimelineStopped);

                var startDrawingPos = _boardSketcher.Surfaces[i].Get3DPoint(_boardSketcher.Points[startDrawingIndex]);
                var forward = (_boardSketcher.Points[startDrawingIndex] - _boardSketcher.Points[startDrawingIndex + 1]).normalized;
                var endForward = _boardSketcher.Surfaces[i].transform.TransformDirection(new Vector3(forward.x, 0, forward.y));

                stick.pathPlan.PlanPath(stick.start.position, stick.start.forward, startDrawingPos, endForward);
            }
        }
        private void TimelineStopped()
        {
            _timelineCount++;
            if (_timelineCount == _boardSketcher.PenUsageNum)
            {
                BeginDrawing();
                _timelineCount = 0;
            }
        }

        public void BeginDrawing()
        {
            for (var i = 0; i < _boardSketcher.PenUsageNum; i++)
            {
                _boardSketcher.Pens[i].SetPenBall(bambooStickVisualTransforms[i]);
                _boardSketcher.Pens[i].Done += OnSketchingDone;
                _boardSketcher.StartPenDrawing(i, bambooSticks[i].GetMoverSpeed());
            }
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
                    stick.pathPlan.PlanPath(stick.mover.transform.position, stick.mover.transform.forward, endPosition,
                        endForward);
                    // stick.timeline.Play();
                    stick.mover.ResetDisplacement();
                    stick.StartTimelineMoving(null);
                }
            }
        }
    }
}