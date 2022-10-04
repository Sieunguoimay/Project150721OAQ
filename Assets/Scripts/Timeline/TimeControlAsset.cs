using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline
{
    public interface ITimeControlExtended
    {
        /// <summary>
        /// Called each frame the Timeline clip is active.
        /// </summary>
        /// <param name="time">The local time of the associated Timeline clip.</param>
        /// <param name="duration">Duration</param>
        void SetTime(double time, double duration);

        /// <summary>
        /// Called when the associated Timeline clip becomes active.
        /// </summary>
        void OnControlTimeStart();

        /// <summary>
        /// Called when the associated Timeline clip becomes deactivated.
        /// </summary>
        void OnControlTimeStop();
    }

    public class TimeControlAsset : PlayableAsset, IPropertyPreview, ITimelineClipAsset
    {
        [SerializeField] private ExposedReference<GameObject> sourceGameObject;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var sourceObject = sourceGameObject.Resolve(graph.GetResolver());

            var timeControl = sourceObject.GetComponent<ITimeControlExtended>();
            if (timeControl == null) return Playable.Create(graph);

            var root = TimeControlExtendPlayable.Create(graph, timeControl);
            return root;
        }

        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
        }

        /// <summary>
        /// Returns the capabilities of TimelineClips that contain a ControlPlayableAsset
        /// </summary>
        public ClipCaps clipCaps => ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ClipCaps.Looping | ClipCaps.None;
    }

    public class TimeControlExtendPlayable : TimeControlPlayable
    {
        ITimeControlExtended m_timeControl;
        private double _duration;

        bool m_started;

        /// <summary>
        /// Creates a Playable with a TimeControlPlayable behaviour attached
        /// </summary>
        /// <param name="graph">The PlayableGraph to inject the Playable into.</param>
        /// <param name="timeControl"></param>
        /// <returns></returns>
        public static ScriptPlayable<TimeControlExtendPlayable> Create(PlayableGraph graph, ITimeControlExtended timeControl)
        {
            if (timeControl == null)
                return ScriptPlayable<TimeControlExtendPlayable>.Null;

            var handle = ScriptPlayable<TimeControlExtendPlayable>.Create(graph);
            handle.GetBehaviour().Initialize(timeControl);
            return handle;
        }

        /// <summary>
        /// Initializes the behaviour
        /// </summary>
        /// <param name="timeControl">Component that implements the ITimeControl interface</param>
        public void Initialize(ITimeControlExtended timeControl)
        {
            m_timeControl = timeControl;
        }

        /// <summary>
        /// This function is called during the PrepareFrame phase of the PlayableGraph.
        /// </summary>
        /// <param name="playable">The Playable that owns the current PlayableBehaviour.</param>
        /// <param name="info">A FrameData structure that contains information about the current frame context.</param>
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            Debug.Assert(m_started, "PrepareFrame has been called without OnControlTimeStart being called first.");
            if (m_timeControl != null)
                m_timeControl.SetTime(playable.GetTime(), playable.GetDuration());
        }

        /// <summary>
        /// This function is called when the Playable play state is changed to Playables.PlayState.Playing.
        /// </summary>
        /// <param name="playable">The Playable that owns the current PlayableBehaviour.</param>
        /// <param name="info">A FrameData structure that contains information about the current frame context.</param>
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (m_timeControl == null)
                return;

            if (!m_started)
            {
                m_timeControl.OnControlTimeStart();
                m_started = true;
            }
        }

        /// <summary>
        /// This function is called when the Playable play state is changed to PlayState.Paused.
        /// </summary>
        /// <param name="playable">The playable this behaviour is attached to.</param>
        /// <param name="info">A FrameData structure that contains information about the current frame context.</param>
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (m_timeControl == null)
                return;

            if (m_started)
            {
                m_timeControl.OnControlTimeStop();
                m_started = false;
            }
        }
    }
}