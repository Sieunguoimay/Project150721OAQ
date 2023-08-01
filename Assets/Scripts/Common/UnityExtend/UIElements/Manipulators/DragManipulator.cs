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
        private readonly Dragger _dragger;

        public DragManipulator(VisualElement dragContent, Action dragAction = null)
        {
            _dragger = new Dragger(dragContent, dragAction);
        }
        private void SetupEvents()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void OnMouseDown(MouseDownEvent ev)
        {
            if(ev.pressedButtons == 4)
            {
                _dragger.BeginDrag(ev.mousePosition);
                ev.StopPropagation();
            }
        }
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (evt.pressedButtons == 0)
            {
                _dragger.EndDrag();
            }
            else if (evt.pressedButtons == 4)
            {
                _dragger.Drag(evt.mousePosition);
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            _dragger.EndDrag();
        }
    }

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
        public void Drag(Vector2 mousePosition)
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
}