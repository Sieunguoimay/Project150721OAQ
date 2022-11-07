using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public class Activity
    {
        public virtual bool Inactive { get; protected set; } = true;

        public virtual void Begin()
        {
            Inactive = false;
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void End()
        {
        }

        public void MarkAsDone()
        {
            Inactive = true;
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

                if (!_activities[i].Inactive) continue;

                _activities[i].End();

                RemoveAt(i--);
            }

            if (_activities.Count == 0)
            {
                MarkAsDone();
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
        }
    }

    public interface IActivityQueue
    {
        IReadOnlyCollection<Activity> Activities { get; }
        void Add(Activity activity);
    }

    public class ActivityQueue : Activity, IActivityQueue
    {
        private Activity _currentActivity;

        private readonly Queue<Activity> _activities = new();
        public IReadOnlyCollection<Activity> Activities => _activities;

        public void Add(Activity anim)
        {
            if (anim != null)
            {
                _activities.Enqueue(anim);
            }
        }

        public override void Update(float deltaTime)
        {
            if (Inactive) return;

            if (_currentActivity == null)
            {
                if (_activities.Count > 0)
                {
                    _currentActivity = _activities.Dequeue();
                    _currentActivity.Begin();
                }
                else
                {
                    _currentActivity = null;
                }
            }
            else if (_currentActivity.Inactive)
            {
                _currentActivity.End();
                if (_activities.Count > 0)
                {
                    _currentActivity = _activities.Dequeue();
                    _currentActivity.Begin();
                }
                else
                {
                    _currentActivity = null;
                    MarkAsDone();
                }
            }
            else
            {
                _currentActivity.Update(deltaTime);
            }
        }

        public override void End()
        {
            base.End();

            foreach (var a in _activities)
            {
                a.MarkAsDone();
                a.End();
            }

            _currentActivity = null;
            _activities.Clear();
        }
    }
}