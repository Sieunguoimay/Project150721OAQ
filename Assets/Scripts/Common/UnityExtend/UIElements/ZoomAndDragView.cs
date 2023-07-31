using Common.UnityExtend.UIElements.Utilities;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements
{

    public class ZoomAndDragView : VisualElement
    {
        private readonly VisualElement _contentContainer = new() { name = "contents" };
        public VisualElement ContentContainer => _contentContainer;
        private readonly ZoomManipulator _zoomManipulator;
        public ZoomAndDragView(float zoomMin = .25f, float zoomMax = 4f)
        {
            _zoomManipulator = new ZoomManipulator(_contentContainer, zoomMin, zoomMax);
            var drag = new DragManipulator(_contentContainer, MarkDirtyRepaint);
            this.AddManipulator(drag);
            this.AddManipulator(_zoomManipulator);

            _contentContainer.style.position = Position.Absolute;
            Add(_contentContainer);

            RegisterCallback<MouseUpEvent>(OnMouseUp);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            //generateVisualContent += OnCanvasRepaint;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            FocusCenter();
        }

        private void OnCanvasRepaint(MeshGenerationContext obj)
        {
            var pos = new Vector2(_contentContainer.style.left.value.value, _contentContainer.style.top.value.value);
            Painter2DUtility.DrawCrossSign(obj.painter2D, pos, 10, Color.green);

            var _contentRect = CalculateContentRect();
            Painter2DUtility.DrawRect(obj.painter2D, _contentRect, Color.gray);

            var focusContentRect = CalculateFocusContentRect(_contentRect);
            Painter2DUtility.DrawRect(obj.painter2D, focusContentRect, Color.blue);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 1)
            {
                ShowContextMenu();
            }
        }
        protected virtual Rect CalculateContentRect()
        {
            return this.WorldToLocal(VisualElementTransformUtility.CalculateWorldBoundOfChildren(_contentContainer));
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

        public void FocusCenter()
        {
            var virtualContentRect = CalculateContentRect();
            var focusContentRect = CalculateFocusContentRect(virtualContentRect);
            var scale = focusContentRect.width / virtualContentRect.width;

            var originOffset = new Vector2(
                _contentContainer.style.left.value.value - virtualContentRect.x,
                _contentContainer.style.top.value.value - virtualContentRect.y);

            var newOrigin = new Vector2(
                focusContentRect.x + originOffset.x * scale,
                focusContentRect.y + originOffset.y * scale);

            _contentContainer.style.left = newOrigin.x;
            _contentContainer.style.top = newOrigin.y;

            _zoomManipulator.ForceZoom(scale);
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

    }
}