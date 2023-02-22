using System;
using Gameplay.GameInteract.Button;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public interface IActionChooser
    {
        void ShowUp();
        void HideAway();
        event Action DirectionSelectedEvent;
        bool SelectedDirection { get; }
        void SetPositionAndRotation(Vector3 pos, Quaternion rot);
    }

    public class ActionChooser : MonoBehaviour, IActionChooser
    {
        [SerializeField] private ButtonOnGround left;
        [SerializeField] private ButtonOnGround right;

        private ButtonGroup _buttonGroup;
        public event Action DirectionSelectedEvent;
        public bool SelectedDirection { get; private set; }

        private void OnEnable()
        {
            var buttons = new[] {left, right};

            foreach (var bt in buttons)
            {
                bt.ClickedEvent -= OnClicked;
                bt.ClickedEvent += OnClicked;
            }

            _buttonGroup = new ButtonGroup(buttons);
        }

        public void OnDisable()
        {
            foreach (var bt in _buttonGroup.Buttons)
            {
                bt.ClickedEvent -= OnClicked;
            }
        }

        private void OnClicked(IButton obj)
        {
            SelectedDirection = right == (ButtonOnGround) obj;
            DirectionSelectedEvent?.Invoke();
            HideAway();
        }

        public void SetPositionAndRotation(Vector3 pos, Quaternion rot)
        {
            transform.SetPositionAndRotation(pos, rot);
        }

        public void ShowUp()
        {
            _buttonGroup.ShowButtons();
        }

        public void HideAway()
        {
            _buttonGroup.HideButtons();
        }
    }
}