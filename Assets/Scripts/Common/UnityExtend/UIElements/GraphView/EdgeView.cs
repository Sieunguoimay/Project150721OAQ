using Common.UnityExtend.UIElements.Utilities;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements.GraphView
{
    public class EdgeView : VisualElement
    {
        private NodeView _from;
        private NodeView _to;

        private Vector2 _p1;
        private Vector2 _p2;
        private Vector2 _p3;
        private Vector2 _p4;

        public NodeView From => _from;
        public NodeView To => _to;
        public EdgeView()
        {
            style.position = Position.Absolute;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            generateVisualContent += OnRepaint;
        }

        private void OnRepaint(MeshGenerationContext context)
        {
            UpdateEdge();
            Painter2DUtility.DrawPath(context.painter2D, new[] { _p1, _p2, _p3, _p4 }, Color.cyan, 20f);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            this.MarkDirtyRepaint();
        }

        private void OnNodeMove(NodeView view)
        {
            this.MarkDirtyRepaint();
        }

        public void Connect(NodeView from, NodeView to)
        {
            Disconnect();

            _from = from;
            _to = to;
            _from.AddEdge(this);
            _to.AddEdge(this);
            _from.OnMove += OnNodeMove;
            _to.OnMove += OnNodeMove;
        }


        private void UpdateEdge()
        {
            var connector1 = _from.GetEdgeConnector(this);
            var connector2 = _to.GetEdgeConnector(this);

            var pos1 = parent.WorldToLocal(connector1.position);
            var pos2 = parent.WorldToLocal(connector2.position);

            var xMin = Mathf.Min(pos1.x, pos2.x);
            var yMin = Mathf.Min(pos1.y, pos2.y);
            var xMax = Mathf.Max(pos1.x, pos2.x);
            var yMax = Mathf.Max(pos1.y, pos2.y);

            style.left = xMin;
            style.top = yMin;
            style.width = xMax - xMin;
            style.height = yMax - yMin;

            _p1 = this.WorldToLocal(connector1.position);
            _p2 = this.WorldToLocal(connector1.position + connector1.normal * .2f);
            _p3 = this.WorldToLocal(connector2.position + connector2.normal * .2f);
            _p4 = this.WorldToLocal(connector2.position);
        }

        public void Disconnect()
        {
            if (_from != null)
            {
                _from.OnMove -= OnNodeMove;
                _from.RemoveEdge(this);
                _from = null;
            }
            if (_to != null)
            {
                _to.OnMove -= OnNodeMove;
                _to.RemoveEdge(this);
                _to = null;
            }
        }
    }
}