#if UNITY_EDITOR
using Common.UnityExtend.UIElements.GraphView;
using Common.UnityExtend.UIElements.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public partial class AssetDependencyGraph : VisualElement
{
    private readonly GraphView _graphView;
    private readonly HeaderBar _headerBar;
    private FoldableNode _rootNode;

    public bool LoadScriptDependency { get; private set; }

    private class FoldableNode : DependencyNode
    {
        private readonly FoldoutButton _foldOutButton;
        private bool _foldout = false;
        private readonly List<DependencyNode> _subNodes = new();
        private readonly AssetDependencyGraph _graph;

        public FoldableNode(AssetDependencyGraph graph, string path) : base(path)
        {
            _graph = graph;
            LoadChildren();
            if (_subNodes.Count > 0)
            {
                _foldOutButton = new FoldoutButton();
                _foldOutButton.style.position = Position.Absolute;
                _foldOutButton.style.width = 10;
                _foldOutButton.style.height = 10;
                _foldOutButton.style.left = -16;
                _foldOutButton.Clicked += OnFoldoutButtonClicked;
                Add(_foldOutButton);
            }
        }
        private void OnFoldoutButtonClicked(FoldoutButton evt)
        {
            _foldout = !_foldout;
            _foldOutButton.SetRotateDown(_foldout);
            UpdateFoldoutChildren();
        }
        private void UpdateFoldoutChildren()
        {
            if (_foldout)
            {
                for (int i = 0; i < _subNodes.Count; i++)
                {
                    var sn = _subNodes[i];

                    Add(sn);

                    sn.style.left = 10;
                    sn.style.top = (i + 1) * 30;
                }
            }
            else
            {
                foreach (var sn in _subNodes)
                {
                    Remove(sn);
                }
            }
        }
        private void LoadChildren()
        {
            _subNodes.Clear();

            var subObjects = GetSubObjects();
            var dependencies = QueryConnectedNodes(true).ToArray();

            foreach (var subObject in subObjects)
            {
                if (subObject != Target)
                {
                    var subNode = new DependencyNode(subObject);
                    _subNodes.Add(subNode);
                }
            }
        }
        private IEnumerable<Object> GetSubObjects()
        {
            bool isSceneAsset = Path.EndsWith(".unity");
            if (isSceneAsset)
            {
                return new Object[0];
            }
            return AssetDatabase.LoadAllAssetsAtPath(Path).Where(HasReferenceToOutside);
        }

        private bool HasReferenceToOutside(Object obj)
        {
            return GetAllReferences(obj).Any(o => o != Target && AssetDatabase.IsMainAsset(o));
        }
        private IEnumerable<Object> GetAllReferences(Object obj)
        {
            var it = new SerializedObject(obj).GetIterator();
            while (it.Next(true))
            {
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                {
                    yield return it.objectReferenceValue;
                }
            }
        }

        public class FoldoutButton : VisualElement
        {
            private bool _down;
            public event System.Action<FoldoutButton> Clicked;
            public FoldoutButton()
            {
                generateVisualContent += OnRepaint;
                RegisterCallback<MouseDownEvent>(OnMouseDown);
                RegisterCallback<MouseUpEvent>(OnMouseUp);
            }

            private void OnMouseUp(MouseUpEvent evt)
            {
                if (evt.button == 0)
                {
                    if (_down)
                    {
                        _down = false;
                        Clicked?.Invoke(this);
                    }
                    evt.StopPropagation();
                }
            }

            private void OnMouseDown(MouseDownEvent evt)
            {
                if (evt.pressedButtons == 1)
                {
                    _down = true;
                    evt.StopPropagation();
                }
            }

            public void SetRotateDown(bool rotateDown)
            {
                var angle = rotateDown ? (90f) : 0f;
                style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree))); ;
            }

            private void OnRepaint(MeshGenerationContext context)
            {
                Painter2DUtility.FillTriangle(context,
                    new Vector2(0, 0),
                    new Vector2(contentRect.width, contentRect.height / 2f),
                    new Vector2(0, contentRect.height),
                    Color.gray);
            }
        }
    }

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
        var rootNode = DependencyGraphCreator.CreateGraph(target, !LoadScriptDependency, out var nodes, out var edges);

        _rootNode = new FoldableNode(this,rootNode.Path);

        RedirectConnections(rootNode, _rootNode);

        nodes[nodes.IndexOf(rootNode)] = _rootNode;

        _graphView.ClearAll();

        if (nodes.Count > 0)
        {
            AddNodesAndEdgesToGraphView(nodes, edges);
        }
    }
    private static void RedirectConnections(DependencyNode node, FoldableNode newNode)
    {
        var arr = node.ConnectedEdges.ToArray();
        foreach (var edge in arr)
        {
            if (edge.From == node)
            {
                edge.Connect(newNode, edge.To);
            }
            else
            {
                edge.Connect(edge.From, newNode);
            }
        }
    }

    private void AddNodesAndEdgesToGraphView(List<DependencyNode> nodes, List<DependencyEdge> edges)
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
        var hasDependencies = nv.QueryConnectedNodes(true).Any();
        var hasDependent = nv.QueryConnectedNodes(false).Any();

        if (selectedMany)
        {
            menu.AddItem(new GUIContent("Focus"), false, () => _graphView.FocusContent());
        }
        else
        {
            menu.AddItem(new GUIContent("Focus Node"), false, () => FocusSingleNode(nv));
        }

        if (nv != _rootNode && hasDependencies)
        {
            menu.AddItem(new GUIContent("Go into"), false, () => GoInto(nv));
        }

        if (hasDependencies)
        {
            menu.AddItem(new GUIContent("Select Dependencies"), false, () => SelectDirectConnections(nv, true));
            menu.AddItem(new GUIContent("Select All Dependencies"), false, () => SelectConnectionsRecurr(nv, true));
        }
        if (hasDependent)
        {
            menu.AddItem(new GUIContent("Select Dependents"), false, () => SelectDirectConnections(nv, false));
        }
        menu.ShowAsContext();
    }

    private void FocusSingleNode(NodeView nv)
    {
        _graphView.UnselectAllNodes();
        nv.Select(_graphView);
        _graphView.FocusContent();
    }

    private void SelectDirectConnections(NodeView nv, bool fromThis)
    {
        var dependentNodes = nv.QueryConnectedNodes(fromThis);
        foreach (var n in dependentNodes)
        {
            n.Select(_graphView);
        }
    }
    private void SelectConnectionsRecurr(NodeView nv, bool fromThis)
    {
        var dependentNodes = TraverseConnections(nv, fromThis);
        foreach (var n in dependentNodes)
        {
            n.Select(_graphView);
        }
    }
    private IEnumerable<NodeView> TraverseConnections(NodeView node, bool fromThis)
    {
        var dependentNodes = node.QueryConnectedNodes(fromThis);
        foreach (var n in dependentNodes)
        {
            yield return n;
            foreach (var d in TraverseConnections(n, fromThis))
            {
                yield return d;
            }
        }
    }

    private void GoInto(NodeView nv)
    {
        var dnv = nv.Q<DependencyNode>();
        if (dnv?.Target != null)
        {
            _headerBar.ObjectTrain.Push(dnv.Target);
        }
    }
}
#endif