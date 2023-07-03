using Common.UnityExtend.Attribute;
using Common.UnityExtend.Serialization;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace EventSystem
{
    [Serializable]
    public class EventListener : IEventListener
    {
        [SerializeField, TypeSelector.TypeFilter(typeof(IEvent))]
        private TypeSelector eventType;

        [SerializeField] private UnityEvent onTrigger;

        public event Action<IEvent> OnTrigger;

        public void Register()
        {
            EventSystem.Instance.Register(eventType.GetSelectedType(), this);
        }
        public void Unregister()
        {
            EventSystem.Instance.Unregister(eventType.GetSelectedType(), this);
        }

        public void OnReceiveEvent(IEvent action)
        {
            onTrigger?.Invoke();
            OnTrigger?.Invoke(action);
        }
    }
    public class EventListenerSO : ScriptableObject
    {
        [SerializeField, DrawChildrenOnly]
        private EventListener eventListener;
        public EventListener EventListener => eventListener;
    }
}