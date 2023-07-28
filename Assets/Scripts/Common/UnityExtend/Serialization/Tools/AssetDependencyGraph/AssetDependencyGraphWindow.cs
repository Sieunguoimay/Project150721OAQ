#if UNITY_EDITOR
using Common.UnityExtend.UIElements;
using Sieunguoimay.Serialization.Tools;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        var pieChart = new PieChart();
        var pieChart2 = new PieChart();
        var pieChart3 = new PieChart();

        var zoomView = new ZoomAndDragView();
        zoomView.StretchToParentSize();
        rootVisualElement.Add(zoomView);

        zoomView.ContentContainer.Add(pieChart);
        zoomView.ContentContainer.Add(pieChart2);
        zoomView.ContentContainer.Add(pieChart3);

        pieChart.style.position = Position.Absolute;
        pieChart2.style.position = Position.Absolute;
        pieChart3.style.position = Position.Absolute;

        pieChart.style.left = 20;
        pieChart.style.top = 50;
        pieChart2.style.left = -40;
        pieChart2.style.top = 40;
    }

    public static StyleCursor CreateCursor(MouseCursor cursor)
    {
        var objCursor = new UnityEngine.UIElements.Cursor();
        PropertyInfo fields = typeof(UnityEngine.UIElements.Cursor).GetProperty("defaultCursorId", BindingFlags.NonPublic | BindingFlags.Instance);
        fields.SetValue(objCursor, (int)cursor);
        return new StyleCursor(objCursor);
    }

    private void OnGUI()
    {
    }

    public class EventCanvas : VisualElement
    {
        private VisualElement _container;
        public VisualElement Container => _container;
        public EventCanvas()
        {
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
        style.width = 100;
        style.height = 100;
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