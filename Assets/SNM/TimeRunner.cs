using UnityEngine;

namespace SNM
{
    public class TimeRunner
    {
        private bool _moving = false;
        private float _time = 0f;
        private float _duration = 0f;
        private IListener _listener;

        public TimeRunner(IListener listener)
        {
            _listener = listener;
        }

        public void Begin(float duration)
        {
            _moving = true;
            _duration = 2f;
            _time = 0f;
        }

        public void Update(float deltaTime)
        {
            if (_moving)
            {
                if (_time < _duration)
                {
                    _time += deltaTime;
                    float t = Mathf.Min(_time / _duration, 1f);
                    _listener.HandleTimeRunnerValue(t);
                }
                else
                {
                    _moving = false;
                    _listener.OnComplete();
                }
            }
        }

        public interface IListener
        {
            void HandleTimeRunnerValue(float t);
            void OnComplete();
        }
    }
}