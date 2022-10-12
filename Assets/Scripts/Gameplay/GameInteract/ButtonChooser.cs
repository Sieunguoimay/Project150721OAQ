using System;
using System.Collections;
using System.Collections.Generic;
using SNM;
using TMPro;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public interface IOptionChooser
    {
        void SetupOptions(IReadOnlyList<IChoosingOption> options, Action<int> onResult);
        void Choose(int index);
    }

    public interface IChoosingOption
    {
    }

    public class OptionChooser : IOptionChooser
    {
        private Action<int> _onResult;

        public void SetupOptions(IReadOnlyList<IChoosingOption> options, Action<int> onResult)
        {
            _onResult = onResult;
        }

        public void Choose(int index)
        {
            _onResult?.Invoke(index);
        }
    }

    public class ButtonChooser : MonoBehaviour
    {
        [SerializeField] private OnGroundButton[] buttonViews;

        private ButtonData[] _buttons;
        private Action<int> _onResult;

        public void ShowButtons(ButtonData[] buttons, Action<int> onResult)
        {
            _buttons = buttons;
            _onResult = onResult;
            var n = Mathf.Min(buttonViews.Length, _buttons.Length);
            this.TimingForLoop(.3f, n, i =>
            {
                buttonViews[i].RiseUp(_buttons[i].position, _buttons[i].rotation, i, OnButtonClick);
                try
                {
                    buttonViews[i].GetComponentInChildren<TextMeshPro>().text = _buttons[i].displayInfo;
                }
                catch (Exception)
                {
                    //
                }
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
            _onResult?.Invoke(_buttons[obj.ID].ID);
            HideButtons();
        }

        public class ButtonData
        {
            public Vector3 position;
            public Quaternion rotation;
            public string displayInfo;
            public int ID;
        }
    }
}