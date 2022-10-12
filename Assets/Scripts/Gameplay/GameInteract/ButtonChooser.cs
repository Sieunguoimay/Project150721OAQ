using System;
using System.Collections.Generic;
using SNM;
using TMPro;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class ButtonChooser : MonoBehaviour
    {
        [SerializeField] protected OnGroundButton[] buttonViews;

        [field: System.NonSerialized] protected ButtonData[] Buttons { get; set; }
        [field: System.NonSerialized] protected Action<int> Result { get; set; }

        public virtual void Setup(ButtonData[] buttons, Action<int> onResult)
        {
            Buttons = buttons;
            Result = onResult;
            var n = Mathf.Min(buttonViews.Length, Buttons.Length);
            for (var i = 0; i < n; i++)
            {
                buttonViews[i].SetupCallback(i, OnButtonClick);
            }
        }

        public void SetButtonsPositionAndRotation(IReadOnlyList<(Vector3, Quaternion)> posAndRots)
        {
            var n = Mathf.Min(buttonViews.Length, posAndRots.Count);
            for (var i = 0; i < n; i++)
            {
                buttonViews[i].SetPositionAndRotation(posAndRots[i].Item1, posAndRots[i].Item2);
            }
        }

        protected void OnButtonClick(OnGroundButton obj)
        {
            Choose(Buttons[obj.ID].ID);
            HideButtons();
        }

        public void ShowButtons()
        {
            var n = Mathf.Min(buttonViews.Length, Buttons.Length);
            this.TimingForLoop(.3f, n, i => { buttonViews[i].ShowUp(); });
        }

        public void HideButtons()
        {
            var n = Mathf.Min(buttonViews.Length, Buttons.Length);
            for (var i = 0; i < n; i++)
            {
                buttonViews[i].HideAway();
            }
        }

        public void Choose(int index)
        {
            Result?.Invoke(index);
        }

        public class ButtonData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public string DisplayInfo;
            public int ID;
        }
    }
}