using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
{
    public class Dialog : MonoBehaviour
    {
        [SerializeField] private UnityEvent onShow;
        private Transform _currentTarget;

        public void Show(Transform target)
        {
            if (_currentTarget != null && _currentTarget != target)
            {
                ToggleOutline(_currentTarget, false);
            }

            _currentTarget = target;
            
            ToggleOutline(_currentTarget, true);
            
            transform.position = target.position;
            onShow?.Invoke();
        }

        public void Hide()
        {
            if (_currentTarget != null)
            {
                ToggleOutline(_currentTarget, false);
            }
            _currentTarget = null;
        }

        private static void ToggleOutline(Component target, bool toggle)
        {
            target.gameObject.layer = LayerMask.NameToLayer(toggle ? "Outline" : "Default");
        }
    }
}