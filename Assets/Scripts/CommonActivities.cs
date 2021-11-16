using SNM;
using UnityEngine;

namespace CommonActivities
{
    public class StraightMove : Activity
    {
        private Vector3 target;
        private Vector3 origin;
        private float duration;
        private Transform transform;

        private float time;

        public StraightMove(Transform transform, Vector3 target, float duration, IEasing ease = null)
        {
            this.transform = transform;
            this.target = target;
            this.duration = duration;
            if (ease != null)
            {
                SetEase(ease);
            }
        }

        public override void Begin()
        {
            base.Begin();
            time = 0;
            origin = transform.position;
            transform.rotation = Quaternion.LookRotation(SNM.Math.Projection(target - origin,
                Main.Instance.GameCommonConfig.UpVector));
        }

        public override void Update(float deltaTime)
        {
            if (!IsDone)
            {
                time += deltaTime;
                float t = Mathf.Min(time / duration, 1f);
                var pos = Vector3.Lerp(origin, target, Ease.GetEase(t));
                pos.y = transform.position.y;
                transform.position = pos;
                if (time >= duration)
                {
                    IsDone = true;
                }
            }
        }
    }

    public class Delay : SNM.Activity
    {
        private float duration;
        private float time = 0;

        public Delay(float duration)
        {
            this.duration = duration;
        }

        public override void Update(float deltaTime)
        {
            time += deltaTime;
            if (time >= duration)
            {
                IsDone = true;
            }
        }
    }

}