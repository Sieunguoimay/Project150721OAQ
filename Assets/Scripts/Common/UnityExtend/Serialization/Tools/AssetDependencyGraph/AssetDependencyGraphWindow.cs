using Common.UnityExtend.UIElements.GraphView;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AssetDependencyGraphWindow : EditorWindow
{
    private GraphView _graphView;
    private readonly Stack<Object> _stack = new();

    [MenuItem("Tools/AssetDependencyGraphWindow")]
    public static void Open()
    {
        var window = GetWindow<AssetDependencyGraphWindow>("AssetDependencyGraphWindow");
        window.Show();
    }

    private void CreateGUI()
    {
        _graphView = new();
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
        rootVisualElement.Add(CreateHeaderBar());
    }
    private VisualElement CreateHeaderBar()
    {
        var headerBar = new VisualElement();
        headerBar.style.flexDirection = FlexDirection.Row;
        headerBar.style.alignItems = Align.FlexStart;
        headerBar.style.paddingTop = 5;

        var importButton = new Button
        {
            text = "Import"
        };
        importButton.clicked += OnImportButtonClicked;
        importButton.focusable = false;

        var goBackButton = new Button
        {
            text = "<"
        };
        goBackButton.clicked += OnGoBackButtonClicked;
        goBackButton.focusable = false;

        headerBar.Add(importButton);
        headerBar.Add(goBackButton);
        return headerBar;
    }

    private void OnGoBackButtonClicked()
    {
        _stack.TryPop(out var _);
        if(_stack.TryPeek(out var result))
        {
            Import(result);
        }
    }

    private void OnImportButtonClicked()
    {
        _stack.Clear();
        _stack.Push(Selection.activeObject);
        Import(Selection.activeObject);
    }
    private void Import(Object target)
    {
        _graphView.ClearAll();
        CreateGraph(target, out var nodes, out var edges);

        if (nodes.Count == 0 || edges.Count == 0) { return; }

        foreach (var node in nodes)
        {
            _graphView.AddNode(node);
            node.RegisterCallback<GeometryChangedEvent>(OnNodeGeometryChanged);
            node.RegisterCallback<MouseUpEvent>(OnMouseUpInsideNode);
        }
        foreach (var edge in edges)
        {
            _graphView.AddEdge(edge);
            edge.RegisterCallback<GeometryChangedEvent>(OnNodeGeometryChanged);
        }

        void OnNodeGeometryChanged(GeometryChangedEvent evt)
        {
            if (_graphView.Nodes.All(n => n.GeometryReady) && _graphView.Edges.All(n => n.GeometryReady))
            {
                _graphView.FocusCenter();
                _graphView.MarkDirtyRepaint();
            }

            if (evt.target is VisualElement ve)
            {
                ve.UnregisterCallback<GeometryChangedEvent>(OnNodeGeometryChanged);
            }
        }
    }

    private static void CreateGraph(Object rootObject, out List<NodeView> nodes, out List<EdgeView> edges)
    {
        var rootPath = AssetDatabase.GetAssetPath(rootObject);

        edges = new List<EdgeView>();

        var nodeDict = new Dictionary<string, NodeArangementItem>();
        var rootNode = TraverseToCreateGraph(nodeDict, edges, rootPath, new Vector2(0, 0), 0);
        rootNode.depth = 0;
        rootNode.node.style.left = 0;
        rootNode.node.style.top = 0;
        nodes = nodeDict.Values.Select(a => a.node).ToList();

        ArrangeNodes(nodeDict.Values.ToArray());
    }
    private static void ArrangeNodes(IReadOnlyList<NodeArangementItem> arangementItems)
    {
        var columns = arangementItems.OrderBy(i => i.depth).GroupBy(i => i.depth);

        var x = 0f;
        foreach (var column in columns)
        {
            var columnItems = column.ToArray();
            var height = (columnItems.Length - 1) * 60f;
            var parents = columnItems.Where(i => i.parent != null).Select(i => i.parent).Distinct().ToArray();
            var parent = parents.FirstOrDefault();
            var sameParent = parents.Count() <= 1;
            var y = (sameParent && parent != null ? (parent.node.style.top.value.value + parent.node.style.height.value.value / 2f) : 0f) - height / 2f;
            //Debug.Log($"=> {string.Join(",", parents.SelectMany(p => p.node.hierarchy.Children().Select(c => (c as ObjectField).value.name)))}");
            foreach (var g in columnItems)
            {
                g.node.style.left = x;
                g.node.style.top = y;
                y += 60f;
            }

            x += 200f;
        }
    }
    private class NodeArangementItem
    {
        public NodeView node;
        public NodeArangementItem parent;
        public List<NodeArangementItem> children;
        public int depth;
    }

    private static NodeArangementItem TraverseToCreateGraph(Dictionary<string, NodeArangementItem> nodeDict, List<EdgeView> edgeList, string path, Vector2 pos, int depth)
    {
        var parentNode = new NodeArangementItem { node = CreateNode(path), depth = int.MaxValue, children = new() };
        nodeDict.Add(path, parentNode);
        var paths = GetReferences(path);
        foreach (var p in paths)
        {
            if (p.Equals(path)) continue;
            var foundInDict = nodeDict.TryGetValue(p, out var dNode);
            if (!foundInDict)
            {
                dNode = TraverseToCreateGraph(nodeDict, edgeList, p, new Vector2(pos.x + 200f, pos.y), depth + 1);
            }
            if (dNode.depth > depth + 1)
            {
                parentNode.children.Add(dNode);
                dNode.parent = parentNode;
                dNode.depth = depth + 1;

                if (foundInDict)
                {
                    UpdateNodeDepthRecurr(dNode);
                }
            }
            if (dNode != parentNode)
            {
                var edge = new EdgeView();
                edge.Connect(parentNode.node, dNode.node);
                edgeList.Add(edge);
            }
        }

        return parentNode;
    }
    private static void UpdateNodeDepthRecurr(NodeArangementItem item)
    {
        foreach (var c in item.children)
        {
            c.depth = item.depth + 1;
            UpdateNodeDepthRecurr(c);
        }
    }
    private static string[] GetReferences(string path)
    {
        string fullPath = Path.Combine(Application.dataPath, path.Substring("Assets/".Length));

        if (File.Exists(fullPath))
        {
            string assetText = File.ReadAllText(fullPath);
            return ParseReferences(assetText).Distinct()
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !string.IsNullOrEmpty(p) && !AssetDatabase.IsValidFolder(p) && p.Contains('.')).ToArray();
        }
        return new string[0];
    }
    private static IEnumerable<string> ParseReferences(string text)
    {
        var referencePattern = @"\{fileID:\s(-?\d+),\s+guid:\s+([\w\d]+)(?:,\s+type:\s+(\d+))?\}";
        string guidPattern = @"guid:\s+([\w\d]+)";

        var matches = Regex.Matches(text, referencePattern);

        foreach (Match match in matches)
        {
            var m = Regex.Match(match.Value, guidPattern);

            if (m.Success && m.Groups.Count >= 2)
            {
                var guidGroup = m.Groups[1];
                yield return guidGroup.Value;
            }
        }
    }
    private static NodeView CreateNode(string path)
    {
        var node = new NodeView();
        node.style.flexDirection = FlexDirection.Column;
        node.Add(new DependencyNodeVisual(path));
        return node;
    }

    private void OnMouseUpInsideNode(MouseUpEvent evt)
    {
        var selectedNodes = _graphView.Nodes.Where(n => n.IsSelected);
        if (evt.button == 1 && evt.currentTarget is NodeView nv)
        {
            evt.StopPropagation();
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Focus Node"), false, () => FocusSingleNode(nv));
            menu.AddItem(new GUIContent("Select Direct Dependencies"), false, () => SelectDirectDependencies(nv));
            menu.AddItem(new GUIContent("Go into"), false, () => GoInto(nv));
            menu.ShowAsContext();
        }
    }

    private void FocusSingleNode(NodeView nv)
    {
        _graphView.UnselectAllNodes();
        nv.Select(_graphView);
        _graphView.FocusCenter();
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
            _stack.Push(dnv.Target);
            Import(dnv.Target);
        }
    }

    private class DependencyNodeVisual : VisualElement
    {
        private readonly Object _target;
        private readonly Texture _icon;
        private Label _label;
        public Object Target => _target;
        public DependencyNodeVisual(string path)
        {
            _target = AssetDatabase.LoadAssetAtPath<Object>(path);
            _icon = AssetDatabase.GetCachedIcon(path);
            Setup();

            _label.RegisterCallback<MouseDownEvent>(OnMouseDown);
            _label.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            _label.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _label.style.backgroundColor = new Color(.1f, .1f, .1f, .5f);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            _label.style.backgroundColor = new Color(.05f, .05f, .05f, .8f);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.pressedButtons == 1)
            {
                EditorGUIUtility.PingObject(_target);
                evt.StopPropagation();
            }
        }

        private void Setup()
        {
            this.StretchToParentSize();
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;

            var image = new Image
            {
                image = _icon
            };
            image.style.width = 16;
            Add(image);

            var label = new Label
            {
                text = _target.name
            };
            label.style.position = Position.Absolute;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.left = 25;
            label.style.backgroundColor = new Color(.1f, .1f, .1f, .5f);
            Add(label);
            _label = label;
        }

    }
}