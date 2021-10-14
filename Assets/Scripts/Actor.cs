using System.Collections.Generic;
using UnityEngine;

namespace SNM
{
    public interface IEasing
    {
        float GetEase(float x);
    }

    public class LinearEasing : IEasing
    {
        public virtual float GetEase(float x)
        {
            return x;
        }
    }

    public abstract class Activity
    {
        protected IEasing ease = new LinearEasing();
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
            this.ease = linearEasing;
            return this;
        }
    }

    public class ParallelActivity : Activity
    {
        private List<Activity> activities = new List<Activity>();

        public void Add(Activity activity)
        {
            activities.Add(activity);
        }

        public override void Begin()
        {
            base.Begin();
            foreach (var activity in activities)
            {
                activity.Begin();
            }
        }

        public override void Update(float deltaTime)
        {
            for (int i = 0; i < activities.Count; i++)
            {
                activities[i].Update(deltaTime);
                if (activities[i].IsDone)
                {
                    activities[i].End();
                    RemoveAt(i--);
                }
            }

            if (activities.Count == 0)
            {
                IsDone = true;
            }
        }

        private void RemoveAt(int i)
        {
            int lastIndex = activities.Count - 1;
            var last = activities[lastIndex];
            activities[lastIndex] = activities[i];
            activities[i] = last;
            activities.RemoveAt(lastIndex);
        }

        public override void End()
        {
            foreach (var activity in activities)
            {
                activity.End();
            }

            base.End();
            IsDone = true;
        }
    }

    public class SequentialActivity : Activity
    {
        private Queue<Activity> activities = new Queue<Activity>();
        protected Activity currentActivity;

        public void Add(Activity anim)
        {
            activities.Enqueue(anim);
        }

        public override void Update(float deltaTime)
        {
            if (currentActivity == null)
            {
                if (activities.Count > 0)
                {
                    currentActivity = activities.Dequeue();
                    currentActivity.Begin();
                }
                else
                {
                    currentActivity = null;
                }
            }
            else if (currentActivity.IsDone)
            {
                currentActivity.End();
                if (activities.Count > 0)
                {
                    currentActivity = activities.Dequeue();
                    currentActivity.Begin();
                }
                else
                {
                    currentActivity = null;
                    IsDone = true;
                }
            }
            else
            {
                currentActivity.Update(deltaTime);
            }
        }
    }

    public class Actor
    {
        protected Queue<Activity> activities = new Queue<Activity>();
        protected Activity currentActivity;

        public void Add(Activity activity)
        {
            activities.Enqueue(activity);
        }

        public void Update(float deltaTime)
        {
            if (currentActivity == null)
            {
                if (activities.Count > 0)
                {
                    currentActivity = activities.Dequeue();
                    currentActivity.Begin();
                    OnNewActivity(currentActivity);
                }
                else
                {
                    currentActivity = null;
                }
            }
            else if (currentActivity.IsDone)
            {
                currentActivity.End();
                OnActivityEnd(currentActivity);
                if (activities.Count > 0)
                {
                    currentActivity = activities.Dequeue();
                    currentActivity.Begin();
                    OnNewActivity(currentActivity);
                }
                else
                {
                    currentActivity = null;
                }
            }
            else
            {
                currentActivity.Update(deltaTime);
            }
        }

        public void CancelAll()
        {
            foreach (var a in activities)
            {
                a.End();
            }

            activities.Clear();
        }

        protected virtual void OnNewActivity(Activity activity)
        {
        }

        protected virtual void OnActivityEnd(Activity activity)
        {
        }
    }
}