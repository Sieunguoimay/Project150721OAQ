using Common.UnityExtend.UIElements.Utilities;
using System;
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
        private NodeView[] _selectedNodes;
        private bool _selectedSomething;

        public GraphView()
        {
            _selectManipulator = new SelectManipulator();

            _nodeLayer.style.position = Position.Absolute;
            _edgeLayer.style.position = Position.Absolute;

            ContentContainer.Add(_edgeLayer);
            ContentContainer.Add(_nodeLayer);
            this.AddManipulator(_selectManipulator);

            CreateGraph();
            generateVisualContent += OnRepaint;
            this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            this.RegisterCallback<MouseUpEvent>(OnMouseUp);

            _selectManipulator.OnSelectionResult += OnSelectionResult;
        }
        private void OnSelectionResult(SelectManipulator obj)
        {
            this.MarkDirtyRepaint();

            _selectedNodes = _selectManipulator.SelectedElements.OfType<NodeView>().ToArray();

            foreach (var node in _selectedNodes)
            {
                node.Select(this);
            }

            _selectedSomething = _selectedNodes.Length > 0;
        }


        private void OnMouseUp(MouseUpEvent evt)
        {
            foreach (var n in _nodes)
            {
                n.Dragger.EndDrag();
            }

            if(_selectedSomething)
            if (evt.button == 0 && _selectedNodes != null)
            {
                foreach (var node in _selectedNodes)
                {
                    node.Unselect(this);
                }
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (evt.pressedButtons == 0)
            {
                foreach (var n in _nodes)
                {
                    n.Dragger.EndDrag();
                }
                return;
            }
            foreach (var n in _nodes)
            {
                n.Dragger.Drag(evt.mousePosition);
            }
        }

        private void OnRepaint(MeshGenerationContext obj)
        {
            var _contentRect = CalculateContentRect();
            _contentRect.x -= 20;
            _contentRect.y -= 20;
            _contentRect.width += 40;
            _contentRect.height += 40;
            Painter2DUtility.FillRect(obj.painter2D, _contentRect, new Color(0.1568628f, 0.1568628f, 0.1568628f, 1f));
        }

        protected override Rect CalculateContentRect()
        {
            var nodeLayerWorldBould = VisualElementTransformUtility.CalculateWorldBoundOfChildren(_nodeLayer);
            var edgeLayerWorldBould = VisualElementTransformUtility.CalculateWorldBoundOfChildren(_edgeLayer);

            return this.WorldToLocal(VisualElementTransformUtility.CombineBound(nodeLayerWorldBould, edgeLayerWorldBould));
        }

        private void CreateGraph()
        {
            var node = CreateNode();
            var node1 = CreateNode();
            var node3 = CreateNode();

            CreateEdge().Connect(node, node1);
            CreateEdge().Connect(node, node3);
            CreateEdge().Connect(node1, node);
        }

        public NodeView CreateNode()
        {
            var node = new NodeView();
            node.OnMove += OnNodeMove;
            _nodeLayer.Add(node);
            _nodes.Add(node);
            return node;
        }
        public EdgeView CreateEdge()
        {
            var edge = new EdgeView();
            _edgeLayer.Add(edge);
            return edge;
        }

        private void OnNodeMove(NodeView view)
        {
            this.MarkDirtyRepaint();
        }
    }
}