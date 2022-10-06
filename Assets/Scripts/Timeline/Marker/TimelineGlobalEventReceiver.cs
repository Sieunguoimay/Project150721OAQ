using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Timeline.Marker
{
    public class TimelineGlobalEventReceiver : MonoBehaviour, INotificationReceiver
    {
        [SerializeField] private UnityEvent onTrigger;
        public event Action<object> OnTrigger;  
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is GlobalEventMarker glm)
            {
                glm.OnTriggered(origin, notification, context);
                onTrigger?.Invoke();
                OnTrigger?.Invoke(context);
            }
        }
    }
}