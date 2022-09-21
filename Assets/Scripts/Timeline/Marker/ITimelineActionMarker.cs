using UnityEngine.Playables;

namespace Timeline.Marker
{
    public interface ITimelineActionMarker
    {
        void OnTriggered(Playable origin, INotification notification, object context);
    }
}