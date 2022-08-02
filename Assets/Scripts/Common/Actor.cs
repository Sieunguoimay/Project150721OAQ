using System.Collections.Generic;

namespace Common
{
    public abstract class Activity
    {
        public virtual bool IsDone { get; protected set; }

        public virtual void Begin()
        {
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void End()
        {
        }
    }

    public class ParallelActivity : Activity
    {
        private readonly List<Activity> _activities = new();

        public void Add(Activity activity)
        {
            _activities.Add(activity);
        }

        public override void Begin()
        {
            base.Begin();
            foreach (var activity in _activities)
            {
                activity.Begin();
            }
        }

        public override void Update(float deltaTime)
        {
            for (var i = 0; i < _activities.Count; i++)
            {
                _activities[i].Update(deltaTime);

                if (!_activities[i].IsDone) continue;

                _activities[i].End();

                RemoveAt(i--);
            }

            if (_activities.Count == 0)
            {
                IsDone = true;
            }
        }

        private void RemoveAt(int i)
        {
            var lastIndex = _activities.Count - 1;
            var last = _activities[lastIndex];
            _activities[lastIndex] = _activities[i];
            _activities[i] = last;
            _activities.RemoveAt(lastIndex);
        }

        public override void End()
        {
            foreach (var activity in _activities)
            {
                activity.End();
            }

            base.End();
            IsDone = true;
        }
    }

    public class ActivityQueue : Activity
    {
        private Activity _currentActivity;

        public Queue<Activity> Activities { get; } = new();

        public void Add(Activity anim)
        {
            Activities.Enqueue(anim);
        }

        public override void Update(float deltaTime)
        {
            if (_currentActivity == null)
            {
                if (Activities.Count > 0)
                {
                    _currentActivity = Activities.Dequeue();
                    _currentActivity.Begin();
                }
                else
                {
                    _currentActivity = null;
                }
            }
            else if (_currentActivity.IsDone)
            {
                _currentActivity.End();
                if (Activities.Count > 0)
                {
                    _currentActivity = Activities.Dequeue();
                    _currentActivity.Begin();
                }
                else
                {
                    _currentActivity = null;
                    IsDone = true;
                }
            }
            else
            {
                _currentActivity.Update(deltaTime);
            }
        }

        public void CancelAll()
        {
            foreach (var a in Activities)
            {
                a.End();
            }

            Activities.Clear();
        }
    }
}