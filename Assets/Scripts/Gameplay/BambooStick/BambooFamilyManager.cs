using System;
using System.Collections;
using Framework.Resolver;
using Gameplay.Board;
using Gameplay.Board.BoardDrawing;
using UnityEngine;

namespace Gameplay.BambooStick
{
    public class BambooFamilyManager : MonoControlUnitBase<BambooFamilyManager>, GameplayControlUnit.IGameplayUnit
    {
        [SerializeField] private BambooStickSpace[] bambooSticks;
        [SerializeField] private Transform[] bambooStickVisualTransforms;

        private BoardSketcher _boardSketcher;
        private BoardManager _boardManager;
        private int _timelineCount;

        protected override void OnInject(IResolver resolver)
        {
            _boardSketcher = resolver.Resolve<BoardSketcher>();
            _boardManager = resolver.Resolve<BoardManager>();
        }
        public void OnGameplayStart()
        {
            BeginAnimSequence();
        }

        public void OnGameplayStop()
        {
            ForceStop();
        }

        public void ForceStop()
        {
            foreach (var stick in bambooSticks)
            {
                stick.ForceStop();
            }
        }

        public void BeginAnimSequence()
        {
            _boardSketcher.Sketch(_boardManager.Board);
            var points = _boardSketcher.Points;
            var numActivePens = _boardSketcher.PenUsageNum;
            for (var i = 0; i < numActivePens; i++)
            {
                var stick = bambooSticks[i];
                var startDrawingIndex = i * (points.Count / numActivePens);
                stick.StartTimelineMoving(TimelineStopped);

                var startDrawingPos = _boardSketcher.Surfaces[i].Get3DPoint(points[startDrawingIndex]);
                var forward = (points[startDrawingIndex] - points[startDrawingIndex + 1]).normalized;
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