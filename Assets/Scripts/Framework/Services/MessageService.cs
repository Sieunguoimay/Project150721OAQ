using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Services
{
    public class MessageService : IMessageService
    {
        private readonly Dictionary<Type, List<MessageTarget>> _dictionary = new();

        public void Dispatch<TMessage, TSender>(TMessage message, TSender sender = default)
            where TMessage : IMessage<TSender>
        {
            var messageType = typeof(TMessage);
            if (_dictionary.ContainsKey(messageType))
            {
                var list = _dictionary[messageType];
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Sender == null || list[i].Sender.Equals(sender))
                    {
                        (list[i].Handler as IMessageHandler<TMessage, TSender>)?.OnReceiveMessage(message);
                    }
                }
            }
        }

        public void Register<TMessage,TSender>(IMessageHandler<TMessage,TSender> messageHandler, TSender sender = default)
            where TMessage : IMessage<TSender>
        {
            var messageType = typeof(TMessage);
            if (!_dictionary.ContainsKey(messageType))
            {
                _dictionary.Add(messageType,
                    new List<MessageTarget> {new() {Handler = messageHandler, Sender = sender}});
                return;
            }

            if (_dictionary[messageType].Any(t => t.Handler == messageHandler && t.Sender == (object) sender))
            {
                Debug.LogError($"The handler {messageHandler} is already registered!");
                return;
            }

            _dictionary[messageType].Add(new MessageTarget {Handler = messageHandler, Sender = sender});
        }

        public void Unregister<TMessage,TSender>(IMessageHandler<TMessage,TSender> messageHandler, TSender sender = default)
            where TMessage : IMessage<TSender>
        {
            var messageType = typeof(TMessage);

            if (!_dictionary.ContainsKey(messageType))
            {
                Debug.LogError($"Message {messageType.FullName} not exists");
                return;
            }

            var item = _dictionary[messageType].FirstOrDefault(t => t.Handler == messageHandler && t.Sender == (object) sender);
            if (item == null)
            {
                Debug.LogError($"Handler {messageHandler} has not been registered");
                return;
            }

            _dictionary[messageType].Remove(item);
        }

        private class MessageTarget
        {
            public object Sender;
            public object Handler;
        }
    }
}