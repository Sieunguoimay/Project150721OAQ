using UnityEngine;
using UnityEngine.Playables;

namespace Timeline.Marker
{
    [RequireComponent(typeof(PlayableDirector))]
    public class TimelinePauseReceiver: MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is TimelinePauseMarker mk)
            {
                Debug.Log(nameof(TimelinePauseMarker)+ ": Paused at " + mk.time);
                GetComponent<PlayableDirector>().Pause();
            }
        }
    }
}