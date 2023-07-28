using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements
{
    public class ZoomAndDragView : VisualElement
    {
        private readonly ZoomAndDragContainer _contentContainer = new() { name = "contents" };
        private float _scale = 1f;
        private readonly float _scaleMin;
        private readonly float _scaleMax;
        private bool _isDragging;
        private Vector2 _dragBeginContainerPos;
        private Vector2 _dragBeginMousePos;
        public VisualElement ContentContainer => _contentContainer;

        public ZoomAndDragView(float zoomMin = .25f, float zoomMax = 4f)
        {
            _scaleMin = zoomMin;
            _scaleMax = zoomMax;

            _contentContainer.style.position = Position.Absolute;
            Add(_contentContainer);

            RegisterCallback<WheelEvent>(OnWheelEvent);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            generateVisualContent += DrawCanvas;

        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            FocusCenter();
        }

        private void DrawCanvas(MeshGenerationContext obj)
        {
            var pos =
                new Vector2(_contentContainer.style.left.value.value, _contentContainer.style.top.value.value);
            Painter2DUtility.DrawCrossSign(obj.painter2D, pos, 10, Color.green);

            var _contentRect = CalculateContentRect();
            Painter2DUtility.DrawRect(obj.painter2D, _contentRect, Color.gray);

            var focusContentRect = CalculateFocusContentRect(_contentRect);
            Painter2DUtility.DrawRect(obj.painter2D, focusContentRect, Color.blue);
            Debug.Log(focusContentRect);
        }

        private void OnMouseDown(MouseDownEvent ev)
        {
            _isDragging = true;
            _dragBeginContainerPos = new Vector2(_contentContainer.style.left.value.value, _contentContainer.style.top.value.value);
            _dragBeginMousePos = ev.mousePosition;
        }
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (_isDragging)
            {
                Drag(evt.mousePosition);
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            _isDragging = false;
            if (evt.button == 1)
            {
                ShowContextMenu();
            }
        }
        private void ShowContextMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Focus"), false, () =>
            {
                FocusCenter();
            });
            menu.ShowAsContext();
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _isDragging = false;
        }
        public void FocusCenter()
        {
            var currentContentRect = CalculateContentRect();
            var focusContentRect = CalculateFocusContentRect(currentContentRect);
            var scale = focusContentRect.width / currentContentRect.width;

            var originOffset = new Vector2(
                _contentContainer.style.left.value.value - currentContentRect.x,
                _contentContainer.style.top.value.value - currentContentRect.y);
            var newOrigin = new Vector2(
                focusContentRect.x + originOffset.x * scale,
                focusContentRect.y + originOffset.y * scale);
            _contentContainer.style.left = newOrigin.x;
            _contentContainer.style.top = newOrigin.y;

            _scale = Mathf.Clamp(_scale * scale, _scaleMin, _scaleMax);
            _contentContainer.transform.scale = Vector2.one * _scale;

            MarkDirtyRepaint();
        }
        private Rect CalculateContentRect()
        {
            var children = _contentContainer.Children();

            var xMin = children.Min(c => c.style.left.value.value);
            var yMin = children.Min(c => c.style.top.value.value);
            var xMax = children.Max(c => c.style.left.value.value + c.style.width.value.value);
            var yMax = children.Max(c => c.style.top.value.value + c.style.height.value.value);

            return new Rect(_contentContainer.style.left.value.value + xMin * _contentContainer.transform.scale.x,
                _contentContainer.style.top.value.value + yMin * _contentContainer.transform.scale.y,
                _contentContainer.transform.scale.x * Mathf.Max(0, xMax - xMin),
                _contentContainer.transform.scale.y * Mathf.Max(0, yMax - yMin));
        }

        private Rect CalculateFocusContentRect(Rect currentContentRect)
        {
            var parentRect = new Rect(0, 0, contentRect.width, contentRect.height);

            var contentRectRatio = currentContentRect.width / currentContentRect.height;
            var targetRectRatio = parentRect.width / parentRect.height;
            var desireContentRect = new Rect();
            if (contentRectRatio > targetRectRatio)
            {
                desireContentRect.x = parentRect.x;
                desireContentRect.width = parentRect.width;
                desireContentRect.height = parentRect.width * (1f / contentRectRatio);
                desireContentRect.y = parentRect.y + parentRect.height / 2 - desireContentRect.height / 2;
            }
            else
            {
                desireContentRect.y = parentRect.y;
                desireContentRect.width = parentRect.height * contentRectRatio;
                desireContentRect.height = parentRect.height;
                desireContentRect.x = parentRect.x + parentRect.width / 2 - desireContentRect.width / 2;
            }
            return desireContentRect;
        }
        private void Drag(Vector2 targetPosition)
        {
            var delta = targetPosition - _dragBeginMousePos;
            var newContainerPos = _dragBeginContainerPos + delta;

            _contentContainer.style.left = newContainerPos.x;
            _contentContainer.style.top = newContainerPos.y;
            MarkDirtyRepaint();
        }

        private void OnWheelEvent(WheelEvent wheelEvent)
        {
            var localMousePos = wheelEvent.localMousePosition;

            var delta = wheelEvent.delta.y * .1f;

            Zoom(localMousePos, -delta);

        }
        private bool IsZoomValueValid()
        {
            if (_scale >= _scaleMax)
            {
                _scale = _scaleMax;
                return false;
            }
            if (_scale <= _scaleMin)
            {
                _scale = _scaleMin;
                return false;
            }
            return true;
        }
        private void Zoom(Vector2 localPivot, float delta)
        {
            delta *= _scale;

            var changingFactor = delta / _scale;

            _scale += delta;

            if (!IsZoomValueValid())
            {
                return;
            }
            _contentContainer.transform.scale = Vector2.one * _scale;

            var localOrigin = new Vector2(_contentContainer.style.left.value.value, _contentContainer.style.top.value.value);
            var offset = localPivot - localOrigin;
            var move = -offset * changingFactor;
            var newOrigin = localOrigin + move;

            _contentContainer.style.left = newOrigin.x;
            _contentContainer.style.top = newOrigin.y;

            MarkDirtyRepaint();
        }
    }
    public class ZoomAndDragContainer : VisualElement
    {
        public ZoomAndDragContainer()
        {
            generateVisualContent += DrawCanvas;
        }

        private void DrawCanvas(MeshGenerationContext obj)
        {
            Painter2DUtility.DrawCrossSign(obj.painter2D, transform.position, 10, Color.white);
        }
    }
}