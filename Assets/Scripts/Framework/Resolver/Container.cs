using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Resolver
{
    public interface IContainer : IResolver, IBinder
    {
        //Empty
    }

    public class Container : IContainer
    {
        private readonly Dictionary<Type, object> _boundObjects = new();
        private readonly Dictionary<KeyWithId, object> _boundObjectsWithId = new();

        private readonly struct KeyWithId
        {
            public readonly Type Type;
            public readonly string Id;

            public KeyWithId(Type type, string id)
            {
                Type = type;
                Id = id;
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


        public void Bind<TType>(object target, string id)
        {
            var type = typeof(TType);
            if (!_boundObjectsWithId.TryAdd(new KeyWithId(type, id), target))
            {
                Debug.LogError($"Key {type.FullName} - {id} has already been bound to {_boundObjects[type]}");
            }
        }


        public void Unbind<TType>()
        {
            var type = typeof(TType);
            var found = _boundObjects.ContainsKey(type);
            if (!found && !_boundObjects.Remove(type))
            {
                Debug.LogError($"Type {type.FullName} does not exist.");
            }
        }

        public void Unbind<TType>(string id)
        {
            var type = typeof(TType);
            var found = _boundObjectsWithId.ContainsKey(new KeyWithId(type, id));
            if (!found && !_boundObjectsWithId.Remove(new KeyWithId(type, id)))
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
            if (_boundObjectsWithId.TryGetValue(new KeyWithId(type, id), out var obj))
            {
                return (TType) obj;
            }

            Debug.LogError($"Key {type.FullName} - {id} is not bound to any object");
            return default;
        }
    }
}