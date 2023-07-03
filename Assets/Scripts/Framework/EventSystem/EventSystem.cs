
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace EventSystem
{
    public class EventSystem
    {
        private static EventSystem _instance = null;
        public static EventSystem Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new EventSystem();
                _instance.OnInstanceCreated();
                return _instance;
            }
        }
        private Dictionary<System.Type, List<IEventListener>> _actionListenersDict = null;
        private void OnInstanceCreated()
        {
            _actionListenersDict = new Dictionary<System.Type, List<IEventListener>>();
            Debug.Log("EventSystem(singleton) Created");
        }

        public void Dispatch(IEvent action)
        {
            if (!_actionListenersDict.TryGetValue(action.GetType(), out var list)) return;

            for (var i = 0; i < list.Count; i++)
            {
                list[i].OnReceiveEvent(action);
            }
        }

        public void Register(System.Type actionType, IEventListener listener)
        {
            var listenersList = GetOrCreateListenersList(actionType);
            if (!listenersList.Contains(listener))
            {
                listenersList.Add(listener);
            }
        }
        public void Unregister(System.Type actionType, IEventListener listener)
        {
            if (!_actionListenersDict.TryGetValue(actionType, out var list)) return;
            if (list.Contains(listener))
            {
                list.Remove(listener);
            }
        }

        private List<IEventListener> GetOrCreateListenersList(System.Type actionType)
        {
            List<IEventListener> listenersList;
            if (_actionListenersDict.TryGetValue(actionType, out var list))
            {
                listenersList = list;
            }
            else
            {
                listenersList = new List<IEventListener>();
                _actionListenersDict.Add(actionType, listenersList);
            }
            return listenersList;
        }
    }
    public interface IEvent
    {

    }
    public interface IEventListener
    {
        void OnReceiveEvent(IEvent action);
    }
}