using System.Collections.Generic;

namespace SNM.EventSystem
{
    public class EventDispatcher
    {
        private Dictionary<string, List<IEventListener>> _eventListeners =
            new Dictionary<string, List<IEventListener>>();

        private IEventPublisher _publisher;

        public EventDispatcher(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        public void RegisterEvent(
            string eventName, IEventListener listener) //,object extraData
        {
            if (!_eventListeners.ContainsKey(eventName))
            {
                _eventListeners.Add(eventName, new List<IEventListener>());
            }

            _eventListeners[eventName].Add(listener);
        }

        public void Dispatch(string eventName, object data)
        {
            Dispatch(GetEventListeners(eventName), eventName, data);
        }

        public List<IEventListener> GetEventListeners(string eventName)
        {
            if (_eventListeners.ContainsKey(eventName))
                return _eventListeners[eventName];
            return null;
        }

        public void Dispatch(List<IEventListener> eventListeners, string eventName, object extraData)
        {
            if (eventListeners == null) return;
            foreach (var l in eventListeners)
            {
                l.EventResult(_publisher, eventName, extraData);
            }
        }

        public interface IEventListener
        {
            void EventResult(IEventPublisher publisher, string eventName, object data);
        }

        public interface IEventPublisher
        {
            EventDispatcher GetEventDispatcher();
        }
    }
}