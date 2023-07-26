#if UNITY_EDITOR
using Sieunguoimay.Serialization.Tools;
using UnityEditor;
using UnityEngine.UIElements;

public class AssetDependencyGraphWindow : EditorWindow
{
    private AssetDependencyGraphView _graphView;

    [MenuItem("Tools/AssetDependencyGraphWindow")]
    public static void OpenWindow()
    {
        var window = GetWindow<AssetDependencyGraphWindow>(false, "AssetDependencyGraphWindow", true);
        window.Show();
    }

    private void OnEnable()
    {
        _graphView = new AssetDependencyGraphView();

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }
    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }

    private void OnGUI()
    {
    }
}
#endif