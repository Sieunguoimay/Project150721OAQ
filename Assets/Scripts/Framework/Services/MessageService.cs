using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Services
{
    public class MessageService : IMessageService
    {
        private readonly Dictionary<string, List<Action<object, EventArgs>>> _dictionary = new();

        public void Dispatch(string messageType, object sender, EventArgs eventArgs)
        {
            if (!_dictionary.ContainsKey(messageType)) return;

            var list = _dictionary[messageType];

            foreach (var t in list)
            {
                t?.Invoke(sender, eventArgs);
            }
        }

        public void Register(string messageType, Action<object, EventArgs> messageHandler)
        {
            if (!_dictionary.ContainsKey(messageType))
            {
                _dictionary.Add(messageType,
                    new List<Action<object, EventArgs>> { messageHandler });
                return;
            }

            if (_dictionary[messageType].Any(t => t == messageHandler))
            {
                Debug.LogError($"The handler {messageHandler} is already registered!");
                return;
            }

            _dictionary[messageType].Add(messageHandler);
        }

        public void Unregister(string messageType, Action<object, EventArgs> messageHandler)
        {
            if (!_dictionary.ContainsKey(messageType))
            {
                Debug.LogError($"Message {messageType} not exists");
                return;
            }

            var item = _dictionary[messageType].FirstOrDefault(t => t == messageHandler);
            if (item == null)
            {
                Debug.LogError($"Handler {messageHandler} has not been registered");
                return;
            }

            _dictionary[messageType].Remove(item);
        }
    }
}