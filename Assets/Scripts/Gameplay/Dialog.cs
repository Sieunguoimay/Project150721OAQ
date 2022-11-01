using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
{
    public class Dialog : MonoBehaviour
    {
        [SerializeField] private UnityEvent onShow;

        public void Show(Transform target)
        {
            transform.position = target.position;
            onShow?.Invoke();
        }
    }
}