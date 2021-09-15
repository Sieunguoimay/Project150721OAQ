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

    public abstract class Animation
    {
        protected IEasing ease = new LinearEasing();
        public virtual bool IsDone { get; protected set; }

        public virtual void Begin()
        {
        }

        public abstract void Update(float deltaTime);

        public virtual void End()
        {
        }

        public Animation SetEase(IEasing linearEasing)
        {
            this.ease = linearEasing;
            return this;
        }
    }

    public class ParallelAnimation : Animation
    {
        private List<Animation> animations = new List<Animation>();

        public void Add(Animation anim)
        {
            animations.Add(anim);
        }

        public override void Begin()
        {
            base.Begin();
            foreach (var anim in animations)
            {
                anim.Begin();
            }
        }

        public override void Update(float deltaTime)
        {
            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].Update(deltaTime);
                if (animations[i].IsDone)
                {
                    animations[i].End();
                    RemoveAt(i--);
                }
            }

            if (animations.Count == 0)
            {
                IsDone = true;
            }
        }

        private void RemoveAt(int i)
        {
            int lastIndex = animations.Count - 1;
            var last = animations[lastIndex];
            animations[lastIndex] = animations[i];
            animations[i] = last;
            animations.RemoveAt(lastIndex);
        }

        public override void End()
        {
            foreach (var anim in animations)
            {
                anim.End();
            }

            base.End();
            IsDone = true;
        }
    }

    public class SequentialAnimation : Animation
    {
        private Queue<Animation> animations = new Queue<Animation>();
        protected Animation currentAnim;

        public void Add(Animation anim)
        {
            animations.Enqueue(anim);
        }

        public override void Update(float deltaTime)
        {
            if (currentAnim == null)
            {
                if (animations.Count > 0)
                {
                    currentAnim = animations.Dequeue();
                    currentAnim.Begin();
                }
                else
                {
                    currentAnim = null;
                }
            }
            else if (currentAnim.IsDone)
            {
                currentAnim.End();
                if (animations.Count > 0)
                {
                    currentAnim = animations.Dequeue();
                    currentAnim.Begin();
                }
                else
                {
                    currentAnim = null;
                    IsDone = true;
                }
            }
            else
            {
                currentAnim.Update(deltaTime);
            }
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