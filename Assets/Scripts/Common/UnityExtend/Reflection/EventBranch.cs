using System;
using System.Linq;
using UnityEditor;
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
                InternalTriggerByIndex(Array.IndexOf(integerKeys, key));
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
            Debug.LogError(
                $"Trying to Trigger() with key of type {type}. The configured key type is {keyType}. Hint: Use the right Entry Method or change Key Type!");
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

#if UNITY_EDITOR
        public void AddItem()
        {
            var n = eventHandlerGroups.Length + 1;
            Array.Resize(ref eventHandlerGroups, n);
            Array.Resize(ref integerKeys, n);
            Array.Resize(ref stringKeys, n);
        }
#endif
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(EventBranch))]
    public class EventBranchEditor : Editor
    {
        private SerializedProperty _keyType;
        private SerializedProperty _stringKeys;
        private SerializedProperty _integerKeys;
        private SerializedProperty _eventHandlerGroups;

        private void OnEnable()
        {
            _keyType = serializedObject.FindProperty("keyType");
            _stringKeys = serializedObject.FindProperty("stringKeys");
            _integerKeys = serializedObject.FindProperty("integerKeys");
            _eventHandlerGroups = serializedObject.FindProperty("eventHandlerGroups");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(new GUIContent("Script"), MonoScript.FromMonoBehaviour(target as MonoBehaviour),
                typeof(EventBranch), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(_keyType);

            EditorGUILayout.LabelField("Branches");

            var boxStyle = new GUIStyle(GUI.skin.box) {border = new RectOffset(1, 1, 1, 1)};
            EditorGUILayout.BeginVertical(boxStyle);
            var count = _eventHandlerGroups.arraySize;
            for (var i = 0; i < count; i++)
            {
                if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.Index)
                {
                    EditorGUILayout.LabelField($"Index_{i}");
                }

                EditorGUILayout.PropertyField(_eventHandlerGroups.GetArrayElementAtIndex(i));
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                AddItem();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddItem()
        {
            (target as EventBranch)?.AddItem();
        }
    }

    [CustomPropertyDrawer(typeof(EventBranch.EventHandlerGroup))]
    public class EventHandlerGroupDrawer : PropertyDrawer
    {
        private bool _expand;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // base.OnGUI(position, property, label);
            var prop = property.FindPropertyRelative(nameof(EventBranch.EventHandlerGroup.eventHandlers));
            position.height /= (_expand ? (prop.arraySize + 1) : 0) + 1;
            _expand = EditorGUI.Foldout(position, _expand, prop.name,true);
            if (_expand)
            {
                for (var i = 0; i < prop.arraySize; i++)
                {
                    position.y += position.height;
                    EditorGUI.PropertyField(position, prop.GetArrayElementAtIndex(i));
                }

                position.y += position.height;
                position.x += position.width - 22;
                position.width = 20;
                if (GUI.Button(position, "+"))
                {
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var arr = property.FindPropertyRelative(nameof(EventBranch.EventHandlerGroup.eventHandlers));
            return base.GetPropertyHeight(property, label) * ((_expand ? (arr.arraySize + 1) : 0) + 1);
        }
    }
#endif
}