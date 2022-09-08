using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Marker
{
    public class TimelinePauseMarker : UnityEngine.Timeline.Marker, INotification, INotificationOptionProvider
    {
        public PropertyName id => new();
        public NotificationFlags flags => NotificationFlags.TriggerInEditMode;
    }
}