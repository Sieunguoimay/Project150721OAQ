#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
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
        var zoomView = new Common.UnityExtend.UIElements.GraphView.GraphView();
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