using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Marker
{
    public class GlobalEventMarker : UnityEngine.Timeline.Marker, INotification, INotificationOptionProvider
    {
        public PropertyName id => new();
        public NotificationFlags flags => NotificationFlags.TriggerInEditMode;
        public void OnTriggered(Playable origin, INotification notification, object context)
        {
        }
    }
}