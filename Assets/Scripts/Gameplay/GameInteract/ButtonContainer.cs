using System;
using System.Collections.Generic;
using System.Linq;
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
        void Setup(IReadOnlyList<OnGroundButton> buttons);
        void TearDown();
        void ShowButtons();
        void HideButtons();
        IReadOnlyList<OnGroundButton> Buttons { get; }
        event Action<IButtonContainer> AllButtonHiddenEvent;
        event Action<IButtonContainer> AllButtonShownEvent;
    }

    public sealed class ButtonContainer : MonoBehaviour, IButtonContainer
    {
        public IReadOnlyList<OnGroundButton> Buttons { get; private set; }
        public event Action<IButtonContainer> AllButtonHiddenEvent;
        public event Action<IButtonContainer> AllButtonShownEvent;

        private Coroutine _coroutine;
        private int _showCount;
        private int _showAmount;

        public void Setup(IReadOnlyList<OnGroundButton> buttons)
        {
            Buttons = buttons;
            foreach (var b in Buttons)
            {
                b.ClickedEvent -= OnButtonClicked;
                b.ClickedEvent += OnButtonClicked;
                b.ActiveChangedEvent -= OnButtonActiveChanged;
                b.ActiveChangedEvent += OnButtonActiveChanged;
            }
        }

        public void TearDown()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            foreach (var b in Buttons)
            {
                b.ActiveChangedEvent -= OnButtonActiveChanged;
                b.ClickedEvent -= OnButtonClicked;
            }
        }


        private void OnButtonActiveChanged(IButton obj)
        {
            if (!obj.IsShowing)
            {
                AllButtonHiddenEvent?.Invoke(this);
            }
            else
            {
                _showCount++;
                if (_showCount == _showAmount)
                {
                    AllButtonShownEvent?.Invoke(this);
                }
            }
        }

        private void OnButtonClicked(IButton obj)
        {
        }

        public void ShowButtons()
        {
            var availableButtons = Buttons.Where(b => b.IsAvailable).ToArray();
            _showAmount = availableButtons.Length;
            _showCount = 0;
            _coroutine = this.TimingForLoop(.3f, _showAmount, i =>
            {
                availableButtons[i].ShowUp();
                _coroutine = null;
            });
        }

        public void HideButtons()
        {
            var hideCount = 0;
            foreach (var t in Buttons)
            {
                if (t.IsShowing) hideCount++;
                t.HideAway();
            }

            if (hideCount == 0)
            {
                AllButtonHiddenEvent?.Invoke(this);
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