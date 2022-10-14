using System;
using System.Collections.Generic;
using SNM;
using TMPro;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public sealed class ButtonChooser : MonoBehaviour
    {
        [SerializeField] private OnGroundButton[] buttonViews;
        [field: System.NonSerialized] private int OptionNum { get; set; }
        public OnGroundButton[] ButtonViews => buttonViews;

        public void Setup(int num, ICommand[] commands)
        {
            OptionNum = num;
            var n = Mathf.Min(buttonViews.Length, OptionNum);
            for (var i = 0; i < n; i++)
            {
                buttonViews[i].SetupCallback(i, commands[i]);
            }
        }

        // private void OnButtonClick(OnGroundButton obj)
        // {
        //     // Choose(obj.ID);
        //     HideButtons();
        // }

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

        //
        // public void Choose(int index)
        // {
        //     Result?.Invoke(index);
        // }
        public class ButtonCommand : ICommand
        {
            private readonly ButtonChooser _chooser;

            public ButtonCommand(ButtonChooser chooser)
            {
                _chooser = chooser;
            }

            public virtual void Execute()
            {
                _chooser.HideButtons();
            }
        }
    }
}