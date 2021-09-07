using System.Collections.Generic;
using UnityEngine;

namespace SNM
{
    public class Ease
    {
        public virtual float GetEase(float t)
        {
            return t;
        }
    }

    public class InOutExpo : Ease
    {
        public override float GetEase(float t)
        {
            if (t == 0)
            {
                return 0;
            }

            if (t == 1)
            {
                return 1;
            }

            if (t < 0.5) return Mathf.Pow(2f, 20f * t - 10f) / 2f;
            return (2f - Mathf.Pow(2f, -20f * t + 10)) / 2f;
        }
    }

    public abstract class Animation
    {
        protected Ease ease = new Ease();
        public virtual bool IsDone { get; protected set; }

        public virtual void Begin()
        {
        }

        public abstract void Update(float deltaTime);

        public virtual void End()
        {
        }

        public Animation SetEase(Ease ease)
        {
            this.ease = ease;
            return this;
        }
    }

    public class Animator
    {
        protected Queue<Animation> animations = new Queue<Animation>();
        protected Animation currentAnim;

        public void Add(Animation anim)
        {
            animations.Enqueue(anim);
        }

        public void Update(float deltaTime)
        {
            if (currentAnim == null)
            {
                if (animations.Count > 0)
                {
                    currentAnim = animations.Dequeue();
                    currentAnim.Begin();
                    OnNewAnim(currentAnim);
                }
                else
                {
                    currentAnim = null;
                }
            }
            else if (currentAnim.IsDone)
            {
                currentAnim.End();
                OnAnimEnd(currentAnim);
                if (animations.Count > 0)
                {
                    currentAnim = animations.Dequeue();
                    currentAnim.Begin();
                    OnNewAnim(currentAnim);
                }
                else
                {
                    currentAnim = null;
                }
            }
            else
            {
                currentAnim.Update(deltaTime);
            }
        }

        public void CancelAll()
        {
            foreach (var a in animations)
            {
                a.End();
            }

            animations.Clear();
        }

        protected virtual void OnNewAnim(Animation anim)
        {
        }

        protected virtual void OnAnimEnd(Animation anim)
        {
        }
    }
}