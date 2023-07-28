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
        var eventCanavs = new EventCanvas();
        var pieChart = new PieChart();
        eventCanavs.Container.Add(pieChart);
        pieChart.StretchToParentSize();
        eventCanavs.StretchToParentSize();
        rootVisualElement.Add(eventCanavs);
    }

    private void OnGUI()
    {
    }

    public class EventCanvas : VisualElement
    {
        private VisualElement _container;
        public VisualElement Container=>_container;
        public EventCanvas() {
            RegisterCallback<WheelEvent>(OnWheel);
            _container = new VisualElement();
            _container.StretchToParentSize();
            Add(_container);
        }

        public void OnWheel(WheelEvent evt)
        {
            var position = _container.transform.position;
            var scale = _container.transform.scale;
            var vector2 = this.ChangeCoordinatesTo(this, evt.localMousePosition);
            var x = vector2.x + _container.layout.x;
            var y = vector2.y + _container.layout.y;
            var vector3 = position + Vector3.Scale(new Vector3(x, y, 0), scale);

            var newZoom = scale.y - evt.delta.y * .01f;
            var newScale = Vector3.one * newZoom;

            var newPosition = vector3 - Vector3.Scale(new Vector3(x, y, 0), newScale);
            _container.transform.position = newPosition;
            _container.transform.scale = newScale;
        }
    }
}
public class PieChart : VisualElement
{
    public PieChart()
    {
        generateVisualContent += DrawCanvas;
    }
    private void DrawCanvas(MeshGenerationContext context)
    {
        float width = contentRect.width;
        float height = contentRect.height;
        var painter = context.painter2D;

        painter.strokeColor = Color.red;
        painter.lineCap = LineCap.Butt;
        painter.lineWidth = 2;

        var p1 = new Vector2(0, 0);
        var p2 = new Vector2(0, height);
        var p3 = new Vector2(width, height);
        var p4 = new Vector2(width, 0);
        painter.BeginPath();
        painter.MoveTo(p1);
        painter.ArcTo(p2, p3, 20f);
        painter.MoveTo(p3);
        painter.ArcTo(p3, p4, 20f);
        painter.MoveTo(p4);
        painter.ArcTo(p4, p1, 20f);
        painter.Stroke();
    }
}
#endif