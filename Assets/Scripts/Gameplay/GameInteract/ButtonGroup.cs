using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.GameInteract.Button;
using SNM;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public sealed class ButtonGroup : MonoBehaviour
    {
        public IReadOnlyList<ButtonOnGround> Buttons { get; private set; }
        public event Action AllButtonHiddenEvent;
        public event Action AllButtonShownEvent;

        private Coroutine _coroutine;

        public void Setup(IReadOnlyList<ButtonOnGround> buttons)
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
            if (Buttons.All(b => !b.IsShowing)) AllButtonHiddenEvent?.Invoke();
            else if (Buttons.All(b => b.IsShowing)) AllButtonShownEvent?.Invoke();
        }

        public void ShowButtons()
        {
            // _coroutine = this.TimingForLoop(.3f, Buttons.Count, i =>
            // {
            //     Buttons[i].ShowUp();
            //     Debug.Log($"{i}/{Buttons.Count}");
            //     _coroutine = null;
            // });
            foreach (var t in Buttons)
            {
                t.ShowUp();
            }
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