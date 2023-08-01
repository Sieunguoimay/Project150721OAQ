using Common.UnityExtend.UIElements.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements.GraphView
{
    public class GraphView : ZoomAndDragView
    {
        private readonly VisualElement _nodeLayer = new() { name = "node-layer" };
        private readonly VisualElement _edgeLayer = new() { name = "edge-layer" };
        private readonly List<NodeView> _nodes = new();
        private readonly SelectManipulator _selectManipulator;

        public GraphView()
        {
            _selectManipulator = new SelectManipulator();

            _nodeLayer.style.position = Position.Absolute;
            _edgeLayer.style.position = Position.Absolute;

            ContentContainer.Add(_edgeLayer);
            ContentContainer.Add(_nodeLayer);
            this.AddManipulator(_selectManipulator);

            generateVisualContent += OnRepaint;
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseDownEvent>(OnMouseDown);

            _selectManipulator.OnSelectionResult += OnSelectionResult;
        }
        private void OnSelectionResult(SelectManipulator obj)
        {
            this.MarkDirtyRepaint();
            UnselectAllNodes();

            var selectedNodes = _selectManipulator.SelectedElements.OfType<NodeView>().ToArray();

            foreach (var node in selectedNodes)
            {
                node.Select(this);
            }
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.pressedButtons == 1)
            {
                UnselectAllNodes();
            }
        }

        protected override void OnMouseMove(MouseMoveEvent evt)
        {
            base.OnMouseMove(evt);
            foreach (var n in _nodes)
            {
                if (n.IsSelected)
                {
                    n.ProcessMouseMove(evt);
                }
            }
        }

        private void OnRepaint(MeshGenerationContext obj)
        {
            var _contentRect = CalculateContentBound();
            _contentRect.x -= 20;
            _contentRect.y -= 20;
            _contentRect.width += 40;
            _contentRect.height += 40;
            Painter2DUtility.FillRect(obj.painter2D, _contentRect, new Color(0.1568628f, 0.1568628f, 0.1568628f, 1f));
        }

        protected override Rect CalculateFocusBound()
        {
            var selected = _nodes.Where(n => n.IsSelected).ToArray();
            if (selected.Length > 0)
            {
                var worldBound = selected[0].worldBound;
                if (selected.Length > 1)
                {
                    for (int i = 1; i < selected.Length; i++)
                    {
                        worldBound = VisualElementTransformUtility.CombineBound(worldBound, selected[i].worldBound);
                    }
                }
                return this.WorldToLocal(worldBound);
            }

            return CalculateContentBound();
        }
        private Rect CalculateContentBound()
        {
            var nodeLayerWorldBould = VisualElementTransformUtility.CalculateWorldBoundOfChildren(_nodeLayer);
            var edgeLayerWorldBould = VisualElementTransformUtility.CalculateWorldBoundOfChildren(_edgeLayer);

            return this.WorldToLocal(VisualElementTransformUtility.CombineBound(nodeLayerWorldBould, edgeLayerWorldBould));
        }

        public void AddNode(NodeView node)
        {
            node.OnMove += OnNodeMove;
            node.OnClick += OnNodeClick;
            _nodeLayer.Add(node);
            _nodes.Add(node);
        }

        public void RemoveNode(NodeView node)
        {
            node.OnMove -= OnNodeMove;
            node.OnClick -= OnNodeClick;
            _nodeLayer.Remove(node);
            _nodes.Remove(node);
        }

        public void AddEdge(EdgeView edge)
        {
            _edgeLayer.Add(edge);
        }
        public void RemoveEdge(EdgeView edge)
        {
            _edgeLayer.Remove(edge);
        }


        private void OnNodeMove(NodeView view)
        {
            this.MarkDirtyRepaint();
        }
        private void OnNodeClick(NodeView obj)
        {
            if (!obj.IsSelected)
            {
                UnselectAllNodes();
                obj.Select(this);
            }
        }
        private void UnselectAllNodes()
        {
            foreach (var n in _nodes)
            {
                n.Unselect(this);
            }
        }

    }
}