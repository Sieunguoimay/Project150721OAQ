using System;
using System.Linq;
using UnityEngine;

namespace Common.UnityExtend.Reflection
{
    public class EventBranch : MonoBehaviour
    {
        [SerializeField] private BranchKeyType keyType;
        [SerializeField] private string[] stringKeys;
        [SerializeField] private int[] integerKeys;
        [SerializeField] private EventHandlerGroup[] eventHandlerGroups;

        [Serializable]
        public class EventHandlerGroup
        {
            public EventHandlerItem[] eventHandlers;
            private event Action RuntimeAction;

            public void InvokeAction()
            {
                RuntimeAction?.Invoke();
            }

            public void Initialize()
            {
                var handlerType = typeof(Action);

                foreach (var t in eventHandlers)
                {
                    RuntimeAction += (Action) t.CreateDelegate(handlerType);
                }
            }
        }

        private bool _initializedEvents;

        private void Awake()
        {
            if (!_initializedEvents)
            {
                InitializeEvents();
            }
        }

        public void TriggerByIndex(int index)
        {
            if (Validate(BranchKeyType.Index))
            {
                InternalTriggerByIndex(index);
            }
        }

        public void TriggerIntKey(int key)
        {
            if (Validate(BranchKeyType.IntegerKey))
            {
                InternalTriggerByIndex(key);
            }
        }

        public void TriggerStringKey(string key)
        {
            if (!Validate(BranchKeyType.StringIndex)) return;
            InternalTriggerByIndex(Array.IndexOf(stringKeys, key));
        }

        public void TriggerBoolKey(bool key)
        {
            if (Validate(BranchKeyType.BoolKey))
            {
                InternalTriggerByIndex(key ? 1 : 0);
            }
        }

        private void InternalTriggerByIndex(int index)
        {
            if (!_initializedEvents)
            {
                InitializeEvents();
            }

            if ((index < 0 || index > eventHandlerGroups.Length))
            {
                AssertInvalidIndex(index);
            }

            foreach (var g in eventHandlerGroups)
            {
                g.InvokeAction();
            }
        }

        private void InitializeEvents()
        {
            foreach (var g in eventHandlerGroups)
            {
                g.Initialize();
            }

            _initializedEvents = true;
        }

        private bool Validate(BranchKeyType type)
        {
            if (keyType == type) return true;
            Debug.LogError($"Trying to Trigger() with key of type {type}. The configured key type is {keyType}. Hint: Use the right Entry Method or change Key Type!");
            return false;
        }

        private void AssertInvalidIndex(int index)
        {
            Debug.LogError($"Index {index} is invalid. Events {eventHandlerGroups?.Length ?? 0}");
        }

        [Serializable]
        public enum BranchKeyType
        {
            Index,
            BoolKey,
            IntegerKey,
            StringIndex,
        }

        public void Test()
        {
            Debug.Log("Hey");
        }

        [ContextMenu("TestTrigger")]
        public void TestTrigger()
        {
            TriggerByIndex(0);
        }
    }
}