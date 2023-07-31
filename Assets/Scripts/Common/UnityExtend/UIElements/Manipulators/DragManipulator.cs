using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements
{
    public class DragManipulator : IManipulator
    {
        private VisualElement _target;
        public VisualElement target
        {
            get => _target;
            set
            {
                _target = value;
                SetupEvents();
            }
        }
        private readonly VisualElement _dragContent;
        private readonly Action _dragAction;

        private bool _isDragging;
        private Vector2 _dragBeginContainerPos;
        private Vector2 _dragBeginMousePos;

        public DragManipulator(VisualElement dragContent, Action dragAction = null)
        {
            _dragContent = dragContent;
            _dragAction = dragAction;
        }
        private void SetupEvents()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseDown(MouseDownEvent ev)
        {
            _isDragging = true;
            _dragBeginContainerPos = new Vector2(_dragContent.style.left.value.value, _dragContent.style.top.value.value);
            _dragBeginMousePos = _dragContent.parent.WorldToLocal(ev.mousePosition);
            ev.StopPropagation();
        }
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (_isDragging)
            {
                Drag(_dragContent.parent.WorldToLocal(evt.mousePosition));
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            _isDragging = false;
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _isDragging = false;
        }
        private void Drag(Vector2 targetPosition)
        {
            var delta = targetPosition - _dragBeginMousePos;
            var newContainerPos = _dragBeginContainerPos + delta;

            _dragContent.style.left = newContainerPos.x;
            _dragContent.style.top = newContainerPos.y;

            _dragAction?.Invoke();
        }
    }
}