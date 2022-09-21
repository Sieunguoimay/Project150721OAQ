using UnityEngine;
using UnityEngine.Playables;

namespace Timeline.Marker
{
    public class TimelineNotificationReceiver : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is ITimelineActionMarker mk)
            {
                mk.OnTriggered(origin, notification, context);
            }
        }
    }
}