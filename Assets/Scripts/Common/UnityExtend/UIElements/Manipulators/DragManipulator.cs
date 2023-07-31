using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.UIElements
{
    public class Dragger
    {
        private readonly VisualElement _dragContent;
        private readonly Action _dragAction;

        private bool _isDragging;
        private Vector2 _dragBeginContainerPos;
        private Vector2 _dragBeginMousePos;
        public Dragger(VisualElement dragContent, Action dragAction = null)
        {
            _dragContent = dragContent;
            _dragAction = dragAction;
        }

        public void BeginDrag(Vector2 mousePosition)
        {
            _isDragging = true;
            _dragBeginContainerPos = new Vector2(_dragContent.style.left.value.value, _dragContent.style.top.value.value);
            _dragBeginMousePos = _dragContent.parent.WorldToLocal(mousePosition);
        }
        public void UpdateDragIfActive(Vector2 mousePosition)
        {
            if (_isDragging)
            {
                UpdateDrag(_dragContent.parent.WorldToLocal(mousePosition));
            }
        }
        public void EndDrag()
        {
            _isDragging = false;
        }

        private void UpdateDrag(Vector2 targetPosition)
        {
            var delta = targetPosition - _dragBeginMousePos;
            var newContainerPos = _dragBeginContainerPos + delta;

            _dragContent.style.left = newContainerPos.x;
            _dragContent.style.top = newContainerPos.y;

            _dragAction?.Invoke();
        }
    }

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
        public  Dragger Dragger { get; private set; }

        public DragManipulator(VisualElement dragContent, Action dragAction = null)
        {
            Dragger = new Dragger(dragContent, dragAction);
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
            Dragger.BeginDrag(ev.mousePosition);
            ev.StopPropagation();
        }
        private void OnMouseMove(MouseMoveEvent evt)
        {
            Dragger.UpdateDragIfActive(evt.mousePosition);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            Dragger.EndDrag();
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            //Dragger.EndDrag();
        }
    }
}