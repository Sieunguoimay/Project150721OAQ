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
            dialog.GetComponentInChildren<ABoundsClicker>().Clicked.AddListener(OnClick);
        }

        private void OnClick()
        {
            dialog.GetComponentInChildren<ABoundsClicker>().Clicked.RemoveListener(OnClick);
            onClick?.Invoke();
        }
    }
}