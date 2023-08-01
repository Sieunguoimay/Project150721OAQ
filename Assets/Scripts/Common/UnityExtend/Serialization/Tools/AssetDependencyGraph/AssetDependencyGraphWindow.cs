#if UNITY_EDITOR
using Common.UnityExtend.UIElements.GraphView;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class AssetDependencyGraphWindow : EditorWindow
{

    [MenuItem("Tools/AssetDependencyGraphWindow")]
    public static void OpenWindow()
    {
        var window = GetWindow<AssetDependencyGraphWindow>(false, "AssetDependencyGraphWindow", true);
        window.Show();
    }

    private void CreateGUI()
    {
        var zoomView = new GraphView();
        var node = new NodeView();
        zoomView.AddNode(node);

        var colorField = new ColorField();
        colorField.style.width = 70;
        node.Add(colorField);

        zoomView.StretchToParentSize();
        rootVisualElement.Add(zoomView);
    }

    public static StyleCursor CreateCursor(MouseCursor cursor)
    {
        var objCursor = new Cursor();
        PropertyInfo fields = typeof(Cursor).GetProperty("defaultCursorId", BindingFlags.NonPublic | BindingFlags.Instance);
        fields.SetValue(objCursor, (int)cursor);
        return new StyleCursor(objCursor);
    }

    private void OnGUI()
    {
    }
}
#endif