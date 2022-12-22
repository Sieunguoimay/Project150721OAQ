using System;
using SNM;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Dialog
{
    public class DialogCaller : MonoBehaviour
    {
        [SerializeField] private Dialog dialog;
        [SerializeField] private UnityEvent onClick;

        public void ShowDialog()
        {
            dialog.Show(transform);
            dialog.GetComponentInChildren<ABoundsClicker>().Clicked += OnClick;
        }

        private void OnClick(EventArgs eventArgs)
        {
            if (dialog.CurrentTarget == transform)
            {
                dialog.GetComponentInChildren<ABoundsClicker>().Clicked -= OnClick;
                onClick?.Invoke();
            }
        }
    }
}