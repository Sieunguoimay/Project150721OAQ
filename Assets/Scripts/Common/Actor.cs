using System.Collections.Generic;
using UnityEngine;

namespace SNM
{
    public interface IEasing
    {
        float GetEase(float x);
    }

    public sealed class LinearEasing : IEasing
    {
        public float GetEase(float x)
        {
            return x;
        }
    }

    public abstract class Activity
    {
        protected IEasing Ease = new LinearEasing();
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

        public Activity SetEase(IEasing linearEasing)
        {
            this.Ease = linearEasing;
            return this;
        }
    }

    public class ParallelActivity : Activity
    {
        private readonly List<Activity> _activities = new List<Activity>();

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
            for (int i = 0; i < _activities.Count; i++)
            {
                _activities[i].Update(deltaTime);
                if (_activities[i].IsDone)
                {
                    _activities[i].End();
                    RemoveAt(i--);
                }
            }

            if (_activities.Count == 0)
            {
                IsDone = true;
            }
        }

        private void RemoveAt(int i)
        {
            int lastIndex = _activities.Count - 1;
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

    public class SequentialActivity : Activity
    {
        private readonly Queue<Activity> _activities = new Queue<Activity>();
        private Activity _currentActivity;

        public void Add(Activity anim)
        {
            _activities.Enqueue(anim);
        }

        public override void Update(float deltaTime)
        {
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
            else if (_currentActivity.IsDone)
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
                    IsDone = true;
                }
            }
            else
            {
                _currentActivity.Update(deltaTime);
            }
        }
    }

    public class Actor
    {
        private readonly Queue<Activity> _activities = new Queue<Activity>();
        protected Activity CurrentActivity;

        public void Add(Activity activity)
        {
            _activities.Enqueue(activity);
        }

        public void Update(float deltaTime)
        {
            if (CurrentActivity == null)
            {
                if (_activities.Count > 0)
                {
                    CurrentActivity = _activities.Dequeue();
                    CurrentActivity.Begin();
                    OnNewActivity(CurrentActivity);
                }
                else
                {
                    CurrentActivity = null;
                }
            }
            else if (CurrentActivity.IsDone)
            {
                CurrentActivity.End();
                OnActivityEnd(CurrentActivity);
                if (_activities.Count > 0)
                {
                    CurrentActivity = _activities.Dequeue();
                    CurrentActivity.Begin();
                    OnNewActivity(CurrentActivity);
                }
                else
                {
                    CurrentActivity = null;
                }
            }
            else
            {
                CurrentActivity.Update(deltaTime);
            }
        }

        public void CancelAll()
        {
            foreach (var a in _activities)
            {
                a.End();
            }

            _activities.Clear();
        }

        protected virtual void OnNewActivity(Activity activity)
        {
        }

        protected virtual void OnActivityEnd(Activity activity)
        {
        }
    }
}