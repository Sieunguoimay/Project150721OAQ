using System;
using System.Collections;
using System.Collections.Generic;
using SNM;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class ButtonChooser : MonoBehaviour
    {
        [SerializeField] private OnGroundButton[] buttonViews;
        private ButtonData[] _buttons;
        private Action<ButtonData> _onResult;

        public void ShowButtons(ButtonData[] buttons, Action<ButtonData> onResult)
        {
            _buttons = buttons;
            _onResult = onResult;
            var n = Mathf.Min(buttonViews.Length, _buttons.Length);
            this.TimingForLoop(.3f, n, i =>
            {
                buttonViews[i].RiseUp(_buttons[i].position, _buttons[i].rotation, _buttons[i], OnButtonClick);
            });
        }

        public void HideButtons()
        {
            var n = Mathf.Min(buttonViews.Length, _buttons.Length);
            for (var i = 0; i < n; i++)
            {
                buttonViews[i].HideAway();
            }
        }

        private void OnButtonClick(OnGroundButton obj)
        {
            _onResult?.Invoke(obj.AttachedData as ButtonData);
            HideButtons();
        }

        public class ButtonData
        {
            public Vector3 position;
            public Quaternion rotation;
            public int displayInfo;
            public object AttachedData;
        }
    }
}