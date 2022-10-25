using System.Collections.Generic;
using UnityEngine;

namespace System.ResolveSystem
{
    public class Container : IResolver, IBinder
    {
        private readonly Dictionary<Type, object> _boundObjects = new();
        private readonly Dictionary<(Type, string), object> _boundObjectsWithId = new();

        public Container()
        {
            Bind<IBinder>(this);
        }
        
        public void Bind<TType>(TType target)
        {
            var type = typeof(TType);
            if (!_boundObjects.TryAdd(type, target))
            {
                Debug.LogError($"Type {type.FullName} has already been bound to {_boundObjects[type]}");
            }
        }

        public void Bind<TType>(object target)
        {
            var type = typeof(TType);
            if (!_boundObjects.TryAdd(type, target))
            {
                Debug.LogError($"Type {type.FullName} has already been bound to {_boundObjects[type]}");
            }
        }

        public void Bind<TType>(TType target, string id)
        {
            var type = typeof(TType);
            if (!_boundObjectsWithId.TryAdd((type, id), target))
            {
                Debug.LogError($"Key {type.FullName} - {id} has already been bound to {_boundObjects[type]}");
            }
        }

        public void Unbind<TType>(TType target)
        {
            var type = typeof(TType);
            var found = _boundObjects.TryGetValue(type, out var t) && t == (object) target;
            if (!found || !_boundObjects.Remove(type))
            {
                Debug.LogError($"Type {type.FullName} does not exist.");
            }
        }

        public void Unbind<TType>(object target)
        {
            var type = typeof(TType);
            var found = _boundObjects.TryGetValue(type, out var t) && t == (object) target;
            if (!found && !_boundObjects.Remove(type))
            {
                Debug.LogError($"Type {type.FullName} does not exist.");
            }
        }

        public void Unbind<TType>(TType target, string id)
        {
            var type = typeof(TType);
            var found = _boundObjectsWithId.TryGetValue((type, id), out var t) && t == (object) target;
            if (!found && !_boundObjectsWithId.Remove((type, id)))
            {
                Debug.LogError($"Key {type.FullName} - {id} does not exist.");
            }
        }

        public TType Resolve<TType>()
        {
            var type = typeof(TType);
            if (_boundObjects.TryGetValue(type, out var obj))
            {
                return (TType) obj;
            }

            Debug.LogError($"Type {type.FullName} is not bound to any object");
            return default;
        }

        public TType Resolve<TType>(string id)
        {
            var type = typeof(TType);
            if (_boundObjectsWithId.TryGetValue((type, id), out var obj))
            {
                return (TType) obj;
            }

            Debug.LogError($"Key {type.FullName} - {id} is not bound to any object");
            return default;
        }
    }
}