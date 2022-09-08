using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Marker
{
    public class DebugLogMarker : UnityEngine.Timeline.Marker, INotification, INotificationOptionProvider
    {
        [SerializeField] private string text;

        public PropertyName id => new();

        public string Text => text;
        public NotificationFlags flags => NotificationFlags.TriggerInEditMode;
    }
}