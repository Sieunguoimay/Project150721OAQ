using Common.UnityExtend.Attribute;
using UnityEngine;

namespace EventSystem
{
    public class EventListenerMB : MonoBehaviour
    {
        [SerializeField, DrawChildrenOnly]
        private EventListener eventListener;
        public EventListener EventListener => eventListener;
    }
}