using System;
using System.Collections.Generic;
using SNM;
using TMPro;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public sealed class ButtonContainer : MonoBehaviour
    {
        [SerializeField] private OnGroundButton[] buttonViews;
        [field: System.NonSerialized] private int OptionNum { get; set; }
        public OnGroundButton[] ButtonViews => buttonViews;

        public void Setup(ButtonData[] buttons)
        {
            OptionNum = buttons.Length;
            var n = Mathf.Min(buttonViews.Length, OptionNum);
            for (var i = 0; i < n; i++)
            {
                buttonViews[i].SetCommand(buttons[i].Command);
            }
        }

        public void ShowButtons()
        {
            var n = Mathf.Min(buttonViews.Length, OptionNum);
            this.TimingForLoop(.3f, n, i => { buttonViews[i].ShowUp(); });
        }

        public void HideButtons()
        {
            var n = Mathf.Min(buttonViews.Length, OptionNum);
            for (var i = 0; i < n; i++)
            {
                buttonViews[i].HideAway();
                buttonViews[i].SetCommand(null);
            }
        }

        public class ButtonCommand : ICommand
        {
            private readonly ButtonContainer _container;

            public ButtonCommand(ButtonContainer container)
            {
                _container = container;
            }

            public virtual void Execute()
            {
                _container.HideButtons();
            }
        }
    }
}