using Common.UnityExtend.UIElements.Utilities;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements.GraphView
{
    public class GraphView : ZoomAndDragView
    {
        private readonly VisualElement _nodeLayer = new() { name = "node-layer" };
        private readonly VisualElement _edgeLayer = new() { name = "edge-layer" };

        public GraphView()
        {
            _nodeLayer.style.position = Position.Absolute;
            _edgeLayer.style.position = Position.Absolute;

            ContentContainer.Add(_edgeLayer);
            ContentContainer.Add(_nodeLayer);

            CreateGraph();
            generateVisualContent += OnRepaint;
        }

        private void OnRepaint(MeshGenerationContext obj)
        {
            var _contentRect = CalculateContentRect();
            _contentRect.x -= 20;
            _contentRect.y -= 20;
            _contentRect.width += 40;
            _contentRect.height += 40;
            Painter2DUtility.FillRect(obj.painter2D, _contentRect, Color.black);
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