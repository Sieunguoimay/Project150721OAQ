using Common.UnityExtend.UIElements.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements.GraphView
{
    public class NodeView : VisualElement
    {
        private readonly List<EdgeView> _connectedEdges = new();
        public event Action<NodeView> OnMove;
        public NodeView()
        {
            generateVisualContent += OnRepaint;
            style.width = 50;
            style.height = 50;
            style.position = Position.Absolute;
            this.AddManipulator(new DragManipulator(this, OnDrag));
        }
        private void OnDrag()
        {
            OnMove?.Invoke(this);
        }
        private void OnRepaint(MeshGenerationContext context)
        {
            Painter2DUtility.DrawRect(context.painter2D, contentRect, Color.blue);
        }

        public void AddEdge(EdgeView edge)
        {
            _connectedEdges.Add(edge);
        }
        public void RemoveEdge(EdgeView edge)
        {
            _connectedEdges.Remove(edge);
        }

        public EdgeConnector GetEdgeConnector(EdgeView edge)
        {
            var other = edge.From == this ? edge.To : edge.From;
            var otherCenterPoint = this.WorldToLocal(other.LocalToWorld(new Vector2(other.contentRect.x + other.contentRect.width / 2, other.contentRect.y + other.contentRect.height / 2)));
            var connectPoint = this.LocalToWorld(GetIntersectPointFromInsideRect(otherCenterPoint, contentRect, out var normal));

            return new EdgeConnector { normal = normal, position = connectPoint };
        }
        private Vector2 GetIntersectPointFromInsideRect(Vector2 outsidePoint, Rect rect, out Vector2 normal)
        {
            var p1 = new Vector2(rect.x, rect.y);
            var p2 = new Vector2(rect.x + rect.width, rect.y);
            var p3 = new Vector2(rect.x + rect.width, rect.y + rect.height);
            var p4 = new Vector2(rect.x, rect.y + rect.height);
            var centerPoint = new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
            var ps = new[] { p1, p2, p3, p4 };
            for (var i = 0; i < 4; i++)
            {
                var lp1 = ps[i];
                var lp2 = ps[(i + 1) % 4];
                if (VisualElementTransformUtility.IntersectLineSegments2D(lp1, lp2, centerPoint, outsidePoint, out var intersect))
                {
                    normal = Vector2.Perpendicular(lp1 - lp2);
                    return intersect;
                }
            }
            normal = Vector2.up;
            return centerPoint;
        }

        public class EdgeConnector
        {
            public Vector2 position;
            public Vector2 normal;
        }
    }
}