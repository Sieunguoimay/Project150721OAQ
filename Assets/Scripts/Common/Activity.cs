using System;
using System.Collections.Generic;

namespace Common
{
    public abstract class Activity
    {
        public virtual bool IsDone { get; protected set; }
        public event Action Done;
        public virtual void OnBegin()
        {
            IsDone = false;
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void OnEnd()
        {
        }

        protected void NotifyDone()
        {
            IsDone = true;
            Done?.Invoke();
        }
    }

    public class ParallelActivity : Activity
    {
        private readonly List<Activity> _activities = new();

        public void Add(Activity activity)
        {
            _activities.Add(activity);
        }

        public override void OnBegin()
        {
            base.OnBegin();
            foreach (var activity in _activities)
            {
                activity.OnBegin();
            }
        }

        public override void Update(float deltaTime)
        {
            for (var i = 0; i < _activities.Count; i++)
            {
                _activities[i].Update(deltaTime);

                if (!_activities[i].IsDone) continue;

                _activities[i].OnEnd();

                RemoveAt(i--);
            }

            if (_activities.Count == 0)
            {
                NotifyDone();
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

        public override void OnEnd()
        {
            foreach (var activity in _activities)
            {
                activity.OnEnd();
            }

            base.OnEnd();
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
                    _currentActivity.OnBegin();
                }
                else
                {
                    _currentActivity = null;
                }
            }
            else if (_currentActivity.IsDone)
            {
                _currentActivity.OnEnd();
                if (Activities.Count > 0)
                {
                    _currentActivity = Activities.Dequeue();
                    _currentActivity.OnBegin();
                }
                else
                {
                    _currentActivity = null;
                    NotifyDone();
                }
            }
            else
            {
                _currentActivity.Update(deltaTime);
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();
            foreach (var a in Activities)
            {
                a.OnEnd();
            }

            Activities.Clear();
        }
    }
}