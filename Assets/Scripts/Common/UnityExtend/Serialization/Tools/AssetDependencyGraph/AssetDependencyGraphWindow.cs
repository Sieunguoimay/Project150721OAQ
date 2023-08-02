using Common.UnityExtend.UIElements.GraphView;
using System.Collections.Generic;
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
        //rootVisualElement.Add(_graphView);
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
        if (_graphView.parent == rootVisualElement)
        {
            rootVisualElement.Remove(_graphView);
        }
        _graphView.ClearAll();
        Import(Selection.activeObject, _graphView);
        //_graphView.MarkDirtyRepaint();
        rootVisualElement.Add(_graphView);
        _graphView.SendToBack();
    }

    private static void Import(Object rootObject, GraphView graphView)
    {
        var rootPath = AssetDatabase.GetAssetPath(rootObject);

        var dict = new Dictionary<string, NodeView>();

        TraverseToCreateGraph(graphView, dict, rootPath);
    }
    private static NodeView TraverseToCreateGraph(GraphView graphView, Dictionary<string, NodeView> dict, string path)
    {
        var node = CreateNode(AssetDatabase.LoadAssetAtPath<Object>(path));
        graphView.AddNode(node);
        dict.Add(path, node);
        var paths = AssetDatabase.GetDependencies(path);
        foreach (var p in paths)
        {
            if (!dict.TryGetValue(p, out var dNode))
            {
                dNode = TraverseToCreateGraph(graphView, dict, p);
            }
            if (dNode != node)
            {
                var edge = new EdgeView();
                edge.Connect(node, dNode);
                graphView.AddEdge(edge);
            }
        }
        return node;
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
}