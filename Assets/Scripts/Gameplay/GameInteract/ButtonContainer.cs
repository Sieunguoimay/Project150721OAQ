using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.GameInteract.Button;
using SNM;
using UnityEngine;

namespace Gameplay.GameInteract
{
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

        public void Setup(IReadOnlyList<OnGroundButton> buttons)
        {
            Buttons = buttons;
            foreach (var b in Buttons)
            {
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
            }
        }


        private void OnButtonActiveChanged(IButton obj)
        {
            if (Buttons.All(b => !b.IsShowing)) AllButtonHiddenEvent?.Invoke(this);
            else if (Buttons.All(b => b.IsShowing)) AllButtonShownEvent?.Invoke(this);
        }

        public void ShowButtons()
        {
            var availableButtons = Buttons.Where(b => b.IsAvailable).ToArray();
            _coroutine = this.TimingForLoop(.3f, availableButtons.Length, i =>
            {
                availableButtons[i].ShowUp();
                _coroutine = null;
            });
        }

        public void HideButtons()
        {
            foreach (var t in Buttons)
            {
                t.HideAway();
            }
        }
    }
}