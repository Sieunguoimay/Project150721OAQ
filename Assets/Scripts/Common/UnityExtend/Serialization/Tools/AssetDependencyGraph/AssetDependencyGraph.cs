using Common.UnityExtend.UIElements.GraphView;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public partial class AssetDependencyGraph : VisualElement
{
    private readonly GraphView _graphView;
    private readonly HeaderBar _headerBar;
    private NodeView _rootNode;

    public AssetDependencyGraph()
    {
        _graphView = new();
        _headerBar = new(this);
        _graphView.StretchToParentSize();
        Add(_graphView);
        Add(_headerBar);
    }
    private void Import(Object target)
    {
        _rootNode = DependencyGraphCreator.CreateGraph(target, out var nodes, out var edges);

        if (nodes.Count == 0 || edges.Count == 0) { return; }

        _graphView.ClearAll();
        AddNodesAndEdgesToGraphView(nodes, edges);
    }

    private void AddNodesAndEdgesToGraphView(List<NodeView> nodes, List<EdgeView> edges)
    {
        foreach (var node in nodes)
        {
            _graphView.AddNode(node);
            node.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            node.RegisterCallback<MouseUpEvent>(OnMouseUpInsideNode);
        }
        foreach (var edge in edges)
        {
            _graphView.AddEdge(edge);
            edge.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }


        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (_graphView.Nodes.All(n => n.GeometryReady) && _graphView.Edges.All(n => n.GeometryReady))
            {
                _graphView.ContentContainer.MarkDirtyRepaint();
                _graphView.FocusContent();
            }

            if (evt.target is VisualElement ve)
            {
                ve.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
        }
    }

    private void OnMouseUpInsideNode(MouseUpEvent evt)
    {
        if (evt.button == 1 && evt.currentTarget is NodeView nv)
        {
            evt.StopPropagation();
            CreateMenuForNode(nv);
        }
    }

    private void CreateMenuForNode(NodeView nv)
    {
        var menu = new GenericMenu();

        var selectedMany = _graphView.Nodes.Count(n => n.IsSelected) > 1;
        if (selectedMany)
        {
            menu.AddItem(new GUIContent("Focus"), false, () => _graphView.FocusContent());
        }
        else
        {
            menu.AddItem(new GUIContent("Focus Node"), false, () => FocusSingleNode(nv));
        }

        var hasDependencies = nv.QueryConnectedNodes(true).Any();
        if (hasDependencies)
        {
            menu.AddItem(new GUIContent("Select Dependencies"), false, () => SelectDirectDependencies(nv));
        }
        if (nv != _rootNode && hasDependencies)
        {
            menu.AddItem(new GUIContent("Go into"), false, () => GoInto(nv));
        }
        menu.ShowAsContext();
    }

    private void FocusSingleNode(NodeView nv)
    {
        _graphView.UnselectAllNodes();
        nv.Select(_graphView);
        _graphView.FocusContent();
    }

    private void SelectDirectDependencies(NodeView nv)
    {
        var dependentNodes = nv.QueryConnectedNodes(true);
        foreach (var n in dependentNodes)
        {
            n.Select(_graphView);
        }
    }

    private void GoInto(NodeView nv)
    {
        var dnv = nv.Q<DependencyNodeVisual>();
        if (dnv?.Target != null)
        {
            _headerBar.ObjectTrain.Push(dnv.Target);
        }
    }
}