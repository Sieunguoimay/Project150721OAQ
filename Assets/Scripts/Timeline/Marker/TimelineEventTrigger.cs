using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Marker
{
    public class TimelineEventTrigger : UnityEngine.Timeline.Marker, INotification, INotificationOptionProvider, ITimelineActionMarker
    {
        [SerializeField] private UnityEvent onTrigger;
        public PropertyName id => new();
        public NotificationFlags flags => NotificationFlags.TriggerInEditMode;

        public void OnTriggered(Playable origin, INotification notification, object context)
        {
            if (notification is TimelineEventTrigger ttm && ttm == this)
            {
                onTrigger?.Invoke();
            }
        }
    }
}