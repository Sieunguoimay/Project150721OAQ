using UnityEngine;

namespace Implementations
{
    public abstract class AMonoBehaviourWrapper<T> : MonoBehaviour, IFactory<T>
    {
        private T _target;
        private IFactory<T> _factory;
        public T Target => _target != null ? _target : (_target = (_factory ?? this).Create());
        public abstract T Create();

        public void SetFactory(IFactory<T> factory)
        {
            _factory = factory;
        }
    }

    public interface IFactory<T>
    {
        T Create();
    }
}