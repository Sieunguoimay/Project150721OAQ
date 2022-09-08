using UnityEngine;
using UnityEngine.Playables;

namespace Timeline.Marker
{
    public class DebugLogReceiver : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is DebugLogMarker mk)
            {
                Debug.Log(mk.Text + " " + mk.time);
            }
        }
    }
}