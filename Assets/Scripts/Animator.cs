using System.Collections.Generic;

namespace SNM
{
    public interface IAnimation
    {
        bool IsDone { get; }
        void Start();
        void Update(float deltaTime);
        void End();
    }

    public class Animator
    {
        protected Queue<IAnimation> animations = new Queue<IAnimation>();
        protected IAnimation currentAnim;

        public void Add(IAnimation anim)
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
                    currentAnim.Start();
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
                    currentAnim.Start();
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

        protected virtual void OnNewAnim(IAnimation anim)
        {
        }

        protected virtual void OnAnimEnd(IAnimation anim)
        {
        }
    }
}