using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements
{
    public class ZoomManipulator : IManipulator
    {
        public VisualElement _target;
        public VisualElement target
        {
            get => _target;
            set
            {
                _target = value;
                SetupEvents();
            }
        }

        private readonly VisualElement _zoomContent;
        private float _scale = 1f;
        private readonly float _scaleMin;
        private readonly float _scaleMax;

        public ZoomManipulator(VisualElement zoomContent, float zoomMin = .25f, float zoomMax = 4f)
        {
            _scaleMax = zoomMax;
            _scaleMin = zoomMin;
            _zoomContent = zoomContent;
        }
        public void ForceZoom(float scale)
        {
            _scale = Mathf.Clamp(_scale * scale, _scaleMin, _scaleMax);
            _zoomContent.transform.scale = Vector2.one * _scale;
            target.MarkDirtyRepaint();
        }
        private void SetupEvents()
        {
            target.RegisterCallback<WheelEvent>(OnWheelEvent);
        }

        private void OnWheelEvent(WheelEvent wheelEvent)
        {
            var localMousePos = wheelEvent.localMousePosition;

            var delta = wheelEvent.delta.y * .1f;

            Zoom(localMousePos, -delta);

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
            _zoomContent.transform.scale = Vector2.one * _scale;

            var localOrigin = new Vector2(_zoomContent.style.left.value.value, _zoomContent.style.top.value.value);
            var offset = localPivot - localOrigin;
            var move = -offset * changingFactor;
            var newOrigin = localOrigin + move;

            _zoomContent.style.left = newOrigin.x;
            _zoomContent.style.top = newOrigin.y;

            target.MarkDirtyRepaint();
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
    }

}