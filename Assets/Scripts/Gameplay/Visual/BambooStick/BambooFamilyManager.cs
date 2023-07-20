using System;
using Framework.DependencyInversion;
using Framework.Resolver;
using Gameplay.Visual.Board;
using Gameplay.Visual.Board.BoardDrawing;
using UnityEngine;

namespace Gameplay.Visual.BambooStick
{
    public class BambooFamilyManager : MonoBehaviour
    {
        [SerializeField] private BambooStickSpace[] bambooSticks;
        [SerializeField] private Transform[] bambooStickVisualTransforms;
        [SerializeField] private BoardSketcher boardSketcher;

        private int _timelineCount;

        //protected override void OnInject(IResolver resolver)
        //{
            //_boardSketcher = resolver.Resolve<BoardSketcher>();
        //}
        //protected override void OnSetupDependencies()
        //{
        //    base.OnSetupDependencies();
        //}

        public void ResetAll()
        {
            foreach (var stick in bambooSticks)
            {
                stick.ResetState();
            }
            boardSketcher.DeleteDrawing();
            _timelineCount = 0;
        }

        public void BeginAnimSequence(BoardVisual boardVisual)
        {
            boardSketcher.Sketch(boardVisual);
            var points = boardSketcher.Points;
            var numActivePens = boardSketcher.PenUsageNum;
            for (var i = 0; i < numActivePens; i++)
            {
                var stick = bambooSticks[i];
                var startDrawingIndex = i * (points.Count / numActivePens);
                stick.StartTimelineMoving(TimelineStopped);

                var startDrawingPos = boardSketcher.Surfaces[i].Get3DPoint(points[startDrawingIndex]);
                var forward = (points[startDrawingIndex] - points[startDrawingIndex + 1]).normalized;
                var endForward = boardSketcher.Surfaces[i].transform.TransformDirection(new Vector3(forward.x, 0, forward.y));
                stick.pathPlan.PlanPath(stick.start.position, stick.start.forward, startDrawingPos, endForward);
            }
        }

        private void TimelineStopped()
        {
            _timelineCount++;
            if (_timelineCount == boardSketcher.PenUsageNum)
            {
                BeginDrawing();
                _timelineCount = 0;
            }
        }

        public void BeginDrawing()
        {
            for (var i = 0; i < boardSketcher.PenUsageNum; i++)
            {
                boardSketcher.Pens[i].SetPenBall(bambooStickVisualTransforms[i]);
                boardSketcher.Pens[i].Done += OnSketchingDone;
                boardSketcher.StartPenDrawing(i, bambooSticks[i].GetMoverSpeed());
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
            if (_timelineCount == boardSketcher.PenUsageNum)
            {
                for (var i = 0; i < boardSketcher.PenUsageNum; i++)
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