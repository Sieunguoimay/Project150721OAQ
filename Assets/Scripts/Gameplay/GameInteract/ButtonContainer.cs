using System;
using System.Collections.Generic;
using Gameplay.GameInteract.Button;
using SNM;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public interface ICommand
    {
        void Execute(IButton button);
        event Action<ICommand, IButton> ExecutedEvent;
    }

    public interface IButtonContainer
    {
        void Setup(ButtonData[] buttons);
        void ShowButtons();
        void HideButtons();
        OnGroundButton[] Buttons { get; }
    }

    public sealed class ButtonContainer : MonoBehaviour, IButtonContainer
    {
        [SerializeField] private OnGroundButton[] buttonViews;
        [field: System.NonSerialized] private int OptionNum { get; set; }
        public OnGroundButton[] Buttons => buttonViews;

        private Coroutine _coroutine;

        private void OnDisable()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }
        }

        public void Setup(ButtonData[] buttons)
        {
            OptionNum = buttons.Length;
            for (var i = 0; i < buttonViews.Length; i++)
            {
                if (i < OptionNum)
                {
                    buttonViews[i].SetCommand(buttons[i].Command);
                    buttonViews[i].Display?.SetDisplayInfo(buttons[i].DisplayInfo);
                }
                else
                {
                    buttonViews[i].SetCommand(null);
                }
            }

            foreach (var b in Buttons)
            {
                b.ClickedEvent+=OnButtonClicked;
            }
        }

        private void OnButtonClicked(IButton obj)
        {
            
        }

        public void ShowButtons()
        {
            var n = Mathf.Min(buttonViews.Length, OptionNum);
            _coroutine = this.TimingForLoop(.3f, n, i =>
            {
                buttonViews[i].ShowUp();
                _coroutine = null;
            });
        }

        public void HideButtons()
        {
            for (var i = 0; i < Mathf.Min(buttonViews.Length, OptionNum); i++)
            {
                if (!buttonViews[i].Active) continue;
                buttonViews[i].HideAway();
                buttonViews[i].SetCommand(null);
            }
        }

        public class ButtonCommand : ICommand
        {
            private IButtonContainer _container;

            public ButtonCommand SetContainer(IButtonContainer container)
            {
                _container = container;
                return this;
            }

            public virtual void Execute(IButton button)
            {
                _container.HideButtons();
                ExecutedEvent?.Invoke(this, button);
            }

            public event Action<ICommand, IButton> ExecutedEvent;
        }
    }
}