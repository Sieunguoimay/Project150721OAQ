using Common.UnityExtend.UIElements.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements.GraphView
{
    public class NodeView : VisualElement, SelectManipulator.ISelectElement
    {
        private readonly List<EdgeView> _connectedEdges = new();
        public event Action<NodeView> OnMove;
        private bool _hover;
        private bool _selected;
        public Dragger Dragger { get; private set; }
        public NodeView()
        {
            generateVisualContent += OnRepaint;
            style.minWidth = 25;
            style.minHeight = 25;
            style.position = Position.Absolute;
            Dragger = new Dragger(this,OnDrag);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

            style.flexDirection = FlexDirection.Column;

            var colorField = new ColorField();
            colorField.style.width = 70;
            //colorField.SetEnabled(false);
            Add(colorField);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            Dragger.BeginDrag(evt.mousePosition);
            evt.StopPropagation();
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _hover = false;
            this.MarkDirtyRepaint();
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            _hover = true;
            this.MarkDirtyRepaint();
        }

        private void OnDrag()
        {
            OnMove?.Invoke(this);
        }
        private void OnRepaint(MeshGenerationContext context)
        {
            var painter = context.painter2D;

            if (_selected)
            {
                Painter2DUtility.FillAndStrokeRoundedCornerRect(painter, contentRect, new Color(0.2313726f, 0.2313726f, 0.2313726f, 1f), new Color(0.2666667f, 0.7529f, 1f, 1f), 1f);
            }
            else
            {
                Painter2DUtility.FillAndStrokeRoundedCornerRect(painter, contentRect, new Color(0.2313726f, 0.2313726f, 0.2313726f, 1f), new Color(0.09803922f, 0.09803922f, 0.09803922f), .5f);
            }
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
            var cp = new Vector2(other.contentRect.x + other.contentRect.width / 2, other.contentRect.y + other.contentRect.height / 2);
            var otherCenterPoint = this.WorldToLocal(other.LocalToWorld(cp));
            var connectPoint = this.LocalToWorld(GetIntersectPointFromInsideRect(otherCenterPoint, contentRect, out var normal));
            var worldNormal = this.worldTransform.rotation * normal;
            return new EdgeConnector { normal = worldNormal, position = connectPoint };
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


        public void Select(VisualElement selector)
        {
            _selected = true;
            this.MarkDirtyRepaint();
        }

        public void Unselect(VisualElement selector)
        {
            _selected = false;
            this.MarkDirtyRepaint();
        }

        public class EdgeConnector
        {
            public Vector2 position;
            public Vector2 normal;
        }
    }
}