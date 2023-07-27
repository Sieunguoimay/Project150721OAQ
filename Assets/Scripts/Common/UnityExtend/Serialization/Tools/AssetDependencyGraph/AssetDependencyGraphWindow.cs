#if UNITY_EDITOR
using Sieunguoimay.Serialization.Tools;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
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
    private void CreateGUI()
    {
        //_graphView = new AssetDependencyGraphView();
        //_graphView.StretchToParentSize();
        //rootVisualElement.Add(_graphView);
        var pieChart = new PieChart();
        //pieChart.style.left = 50;
        //pieChart.style.width = 150;
        //pieChart.style.height = 150;
        pieChart.StretchToParentSize();
        rootVisualElement.Add(pieChart);
    }

    private void OnGUI()
    {
    }
}
public class PieChart : VisualElement
{
    public PieChart()
    {
        generateVisualContent += DrawCanvas;
        RegisterCallback<WheelEvent>(OnWheel);
    }
    private void OnWheel(WheelEvent evt)
    {
        var position = parent.transform.position;
        var scale = parent.transform.scale;
        var vector2 = this.ChangeCoordinatesTo(parent, evt.localMousePosition);
        var x = vector2.x + parent.layout.x;
        var y = vector2.y + parent.layout.y;
        var vector3 = position + Vector3.Scale(new Vector3(x, y, 0), scale);

        var newZoom = scale.y - evt.delta.y * .1f;
        var newScale = Vector3.one * newZoom;

        var newPosition = vector3 - Vector3.Scale(new Vector3(x, y, 0), newScale);
        parent.transform.position = newPosition;
        parent.transform.scale = newScale;
    }
    private void DrawCanvas(MeshGenerationContext context)
    {
        float width = contentRect.width;
        float height = contentRect.height;
        var painter = context.painter2D;

        painter.strokeColor = Color.red;
        painter.lineCap = LineCap.Butt;
        painter.lineWidth = 2;

        var p1 = new Vector2(painter.lineWidth / 2f, 0);
        var p2 = new Vector2(painter.lineWidth / 2f, height - painter.lineWidth / 2f);
        var p3 = new Vector2(width, height - painter.lineWidth / 2f);
        painter.BeginPath();
        painter.MoveTo(p1);
        painter.ArcTo(p2, p3, 20f);
        painter.LineTo(p3);
        painter.Stroke();
    }
}
#endif