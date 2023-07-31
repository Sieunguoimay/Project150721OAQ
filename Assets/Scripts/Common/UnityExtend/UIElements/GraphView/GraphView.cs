using Common.UnityExtend.UIElements.Utilities;
using System;
using System.Linq;
using System.Xml.Serialization;
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
        }

        protected override Rect CalculateContentRect()
        {
            var nodeLayerWorldBould = VisualElementTransformUtility.CalculateWorldBoundOfChildren(_nodeLayer);
            var edgeLayerWorldBould = VisualElementTransformUtility.CalculateWorldBoundOfChildren(_edgeLayer);

            return this.WorldToLocal(VisualElementTransformUtility.CombineBound(nodeLayerWorldBould, edgeLayerWorldBould));
        }

        private void CreateGraph()
        {
            var node = new NodeView();
            var node1 = new NodeView();
            var node3 = new NodeView();

            node.OnMove += OnNodeMove;
            node1.OnMove += OnNodeMove;
            node3.OnMove += OnNodeMove;

            var edge = new EdgeView();
            edge.Connect(node, node1);
            
            var edge2 = new EdgeView();
            edge2.Connect(node, node3);

            _nodeLayer.Add(node);
            _nodeLayer.Add(node1);
            _nodeLayer.Add(node3);

            _edgeLayer.Add(edge);
            _edgeLayer.Add(edge2);
        }

        private void OnNodeMove(NodeView view)
        {
            this.MarkDirtyRepaint();
        }
    }
}