using UnityEngine;
using UnityEngine.Playables;

namespace Timeline.Marker
{
    public class TimelinePauseReceiver : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is TimelinePauseMarker mk)
            {
                Debug.Log(nameof(TimelinePauseMarker) + ": Paused at " + mk.time);
                (origin.GetGraph().GetResolver() as PlayableDirector)?.Pause();
            }
        }
    }
}