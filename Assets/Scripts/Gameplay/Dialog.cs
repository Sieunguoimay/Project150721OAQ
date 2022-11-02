using Common.UnityExtend.PostProcessing;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
{
    public class LayerToggle : MonoBehaviour
    {
        [SerializeField, LayerSelector] private string selectedLayer;
        [SerializeField, LayerSelector] private string defaultLayer;

        public void ToggleLayer(GameObject target, bool toggle, bool children = true)
        {
            if (children)
            {
                var all = target.GetComponentsInChildren<Transform>();
                foreach (var child in all)
                {
                    child.gameObject.layer = LayerMask.NameToLayer(toggle ? selectedLayer : defaultLayer);
                }
            }
            else
            {
                target.layer = LayerMask.NameToLayer(toggle ? selectedLayer : defaultLayer);
            }
        }
    }

    public class Dialog : LayerToggle
    {
        [SerializeField] private UnityEvent onShow;
        private Transform _currentTarget;

        public void Show(Transform target)
        {
            if (_currentTarget != null && _currentTarget != target)
            {
                ToggleLayer(_currentTarget.gameObject, false);
            }

            _currentTarget = target;

            ToggleLayer(_currentTarget.gameObject, true);

            transform.position = target.position;
            onShow?.Invoke();
        }

        public void Hide()
        {
            if (_currentTarget != null)
            {
                ToggleLayer(_currentTarget.gameObject, false);
            }

            _currentTarget = null;
        }
    }
}