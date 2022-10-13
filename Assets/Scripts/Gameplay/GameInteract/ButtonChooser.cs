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

        [field: System.NonSerialized] private int OptionNum { get; set; }
        [field: System.NonSerialized] private Action<int> Result { get; set; }

        public OnGroundButton[] ButtonViews => buttonViews;

        public virtual void Setup(int num, Action<int> onResult)
        {
            OptionNum = num;
            Result = onResult;
            var n = Mathf.Min(buttonViews.Length, OptionNum);
            for (var i = 0; i < n; i++)
            {
                buttonViews[i].SetupCallback(i, OnButtonClick);
            }
        }

        protected void OnButtonClick(OnGroundButton obj)
        {
            Choose(obj.ID);
            HideButtons();
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
            }
        }

        public void Choose(int index)
        {
            Result?.Invoke(index);
        }
    }
}