using System;
using Gameplay.Visual.GameInteract.Button;
using UnityEngine;

namespace Gameplay.Visual.GameInteract
{
    public class ActionChooser : MonoBehaviour
    {
        [SerializeField] private ButtonOnGround left;
        [SerializeField] private ButtonOnGround right;

        private ButtonGroup _buttonGroup;
        private Action<bool> _directionSelectedHandler;

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
            var selectedDirection = right == (ButtonOnGround) obj;
            _directionSelectedHandler?.Invoke(selectedDirection);
            HideAway();
        }

        public void ShowUp(Action<bool> directionSelectedHandler)
        {
            _directionSelectedHandler = directionSelectedHandler;
            _buttonGroup.ShowButtons();
        }

        public void HideAway()
        {
            _buttonGroup.HideButtons();
        }
    }
}