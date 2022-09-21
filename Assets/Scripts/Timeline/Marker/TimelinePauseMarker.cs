using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Marker
{
    public class TimelinePauseMarker : UnityEngine.Timeline.Marker, INotification, INotificationOptionProvider,ITimelineActionMarker
    {
        public PropertyName id => new();
        public NotificationFlags flags => NotificationFlags.TriggerInEditMode;
        public void OnTriggered(Playable origin, INotification notification, object context)
        {
            Debug.Log(nameof(TimelinePauseMarker) + ": Paused at " + time);
            (origin.GetGraph().GetResolver() as PlayableDirector)?.Pause();
        }
    }
}