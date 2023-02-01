using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Activity
{
    public interface IActivity
    {
        void Begin();
        void End();
    }

    public class Activity : IActivity
    {
        public virtual bool Inactive { get; protected set; } = true;
        public event Action<EventArgs> Done;

        public virtual void Begin()
        {
            Inactive = false;
        }

        public virtual void Update(float deltaTime)
        {
        }

        //<summary>
        //Force End
        //</summary>
        public virtual void End()
        {
            MarkAsDone();
        }

        private void MarkAsDone()
        {
            if (Inactive) return;
            Inactive = true;
            Done?.Invoke(EventArgs.Empty);
        }
    }

    public interface IActivityQueue : IActivity
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
                if (_activities.Count > 0)
                {
                    _currentActivity = _activities.Dequeue();
                    _currentActivity.Begin();
                }
                else
                {
                    _currentActivity = null;
                    End();
                }
            }
            else
            {
                _currentActivity.Update(deltaTime);
            }
        }

        public override void End()
        {
            foreach (var a in _activities.Where(a => !a.Inactive))
            {
                a.End();
            }

            _currentActivity = null;
            _activities.Clear();
            base.End();
        }
    }
}