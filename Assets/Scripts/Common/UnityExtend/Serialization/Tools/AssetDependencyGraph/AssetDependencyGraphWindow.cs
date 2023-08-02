using Common.UnityExtend.UIElements.GraphView;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class AssetDependencyGraphWindow : EditorWindow
{
    private GraphView _graphView;

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
        headerBar.style.alignItems = Align.Center;
        headerBar.style.paddingTop = 5;
        var button = new Button
        {
            text = "Import"
        };
        button.clicked += OnImportButtonClicked;
        button.focusable = false;
        headerBar.Add(button);
        return headerBar;
    }

    private void OnImportButtonClicked()
    {
        _graphView.ClearAll();
        CreateGraph(Selection.activeObject, out var nodes, out var edges);

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
        foreach (var i in arangementItems)
        {
            i.depth = (i.parent?.depth ?? 0) + 1;
        }

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
        public int depth;
    }

    private static NodeArangementItem TraverseToCreateGraph(Dictionary<string, NodeArangementItem> nodeDict, List<EdgeView> edgeList, string path, Vector2 pos, int depth)
    {
        var parentNode = new NodeArangementItem { node = CreateNode(AssetDatabase.LoadAssetAtPath<Object>(path)) };
        nodeDict.Add(path, parentNode);
        var paths = GetReferences(path);
        foreach (var p in paths)
        {
            if (p.Equals(path)) continue;

            if (!nodeDict.TryGetValue(p, out var dNode))
            {
                dNode = TraverseToCreateGraph(nodeDict, edgeList, p, new Vector2(pos.x + 200f, pos.y), depth + 1);
            }
            if (dNode.parent == null || dNode.parent.depth > parentNode.depth)
            {
                dNode.parent = parentNode;
            }
            dNode.depth = depth + 1;
            if (dNode != parentNode)
            {
                var edge = new EdgeView();
                edge.Connect(parentNode.node, dNode.node);
                edgeList.Add(edge);
            }
        }

        return parentNode;
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
    private static NodeView CreateNode(Object target)
    {
        var node = new NodeView();
        var objectField = new ObjectField
        {
            value = target
        };
        node.style.flexDirection = FlexDirection.Column;
        node.style.maxWidth = 100;
        objectField.SetEnabled(false);
        node.Add(objectField);
        return node;
    }

    private void OnMouseUpInsideNode(MouseUpEvent evt)
    {
        var selectedNodes = _graphView.Nodes.Where(n => n.IsSelected);
        //if (selectedNodes.Count() > 1 && selectedNodes.Contains(evt.currentTarget))
        //{
        //    return;
        //}
        if (evt.button == 1 && evt.currentTarget is NodeView nv)
        {
            evt.StopPropagation();
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Focus Node"), false, () => FocusSingleNode(nv));
            menu.AddItem(new GUIContent("Select Direct Dependencies"), false, () => SelectDirectDependencies(nv));
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
}