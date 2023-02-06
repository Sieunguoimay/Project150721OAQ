using System;
using Gameplay.GameInteract.Button;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class ActionChooser : MonoBehaviour
    {
        [SerializeField] private OnGroundButton left;
        [SerializeField] private OnGroundButton right;
        [SerializeField] private ButtonContainer buttonContainer;
        public ButtonContainer ButtonContainer => buttonContainer;

        public event Action<ActionChooser, DirectionSelectArgs> DirectionSelectedEvent;

        public class DirectionSelectArgs : EventArgs
        {
            public DirectionSelectArgs(bool direction)
            {
                Direction = direction;
            }

            public bool Direction { get; }
        }

        public void Setup()
        {
            var buttons = new[] {left, right};
            
            foreach (var bt in buttons)
            {
                bt.ClickedEvent -= OnClicked;
                bt.ClickedEvent += OnClicked;
            }

            buttonContainer.Setup(buttons);
        }

        public void TearDown()
        {
            foreach (var bt in buttonContainer.Buttons)
            {
                bt.ClickedEvent -= OnClicked;
            }

            buttonContainer.TearDown();
        }

        private void OnClicked(IButton obj)
        {
            DirectionSelectedEvent?.Invoke(this, new DirectionSelectArgs(right == (OnGroundButton) obj));
            HideAway();
        }

        public void ShowUp()
        {
            buttonContainer.ShowButtons();
        }

        public void HideAway()
        {
            buttonContainer.HideButtons();
        }
    }
}