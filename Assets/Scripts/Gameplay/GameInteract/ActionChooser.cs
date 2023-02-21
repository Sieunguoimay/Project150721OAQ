using System;
using Gameplay.GameInteract.Button;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class ActionChooser : MonoBehaviour
    {
        [SerializeField] private ButtonOnGround left;
        [SerializeField] private ButtonOnGround right;
        [SerializeField] private ButtonGroup buttonGroup;
        public event Action DirectionSelectedEvent;
        public bool SelectedDirection { get; private set; }

        public void Setup()
        {
            var buttons = new[] {left, right};

            foreach (var bt in buttons)
            {
                bt.ClickedEvent -= OnClicked;
                bt.ClickedEvent += OnClicked;
            }

            buttonGroup.Setup(buttons);
        }

        public void TearDown()
        {
            foreach (var bt in buttonGroup.Buttons)
            {
                bt.ClickedEvent -= OnClicked;
            }

            buttonGroup.TearDown();
        }

        private void OnClicked(IButton obj)
        {
            SelectedDirection = right == (ButtonOnGround) obj;
            DirectionSelectedEvent?.Invoke();
            HideAway();
        }

        public void ShowUp(Transform tileTransform, float offset)
        {
            var tileRotation = tileTransform.rotation;
            var pos = tileTransform.position + tileRotation * Vector3.forward * offset;
            
            transform.SetPositionAndRotation(pos, tileRotation);
            
            buttonGroup.ShowButtons();
        }

        public void HideAway()
        {
            buttonGroup.HideButtons();
        }
    }
}