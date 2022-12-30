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
            if (!Validate(BranchKeyType.StringKey)) return;
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
            StringKey,
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

        private int _foldout;

        private void OnEnable()
        {
            _keyType = serializedObject.FindProperty("keyType");
            _stringKeys = serializedObject.FindProperty("stringKeys");
            _integerKeys = serializedObject.FindProperty("integerKeys");
            _eventHandlerGroups = serializedObject.FindProperty("eventHandlerGroups");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(new GUIContent("Script"), MonoScript.FromMonoBehaviour(target as MonoBehaviour),
                typeof(EventBranch), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(_keyType);

            EditorGUILayout.LabelField("Branches");

            EditorGUILayout.BeginVertical();
            var count = _eventHandlerGroups.arraySize;
            var label = "None";
            for (var i = 0; i < count; i++)
            {
                if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.Index)
                {
                    label = ($"Index {i}");
                }
                else if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.BoolKey)
                {
                    if (i > 2) continue;
                }
                else if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.IntegerKey)
                {
                    label = "Integer";
                }
                else if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.StringKey)
                {
                    label = "String";
                }

                var rect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.Space(15, false);
                var eventHandlers =
                    _eventHandlerGroups.GetArrayElementAtIndex(i)
                        .FindPropertyRelative(nameof(EventBranch.EventHandlerGroup.eventHandlers));

                EditorGUILayout.BeginVertical();
                // _foldout = EditorGUILayout.Foldout(_foldout, new GUIContent(label), true);
                for (int j = 0; j < eventHandlers.arraySize; j++)
                {
                    EditorGUILayout.PropertyField(eventHandlers.GetArrayElementAtIndex(j));
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-"))
                {
                    eventHandlers.DeleteArrayElementAtIndex(eventHandlers.arraySize - 1);
                }

                if (GUILayout.Button("+"))
                {
                    eventHandlers.InsertArrayElementAtIndex(eventHandlers.arraySize);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                // if (EditorGUILayout.PropertyField(eventHandlers, new GUIContent(label), true))
                // {
                //     Debug.Log("OK");
                // }

                EditorGUILayout.Space(2, false);
                EditorGUILayout.EndHorizontal();

                // if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.IntegerKey)
                // {
                //     rect.y += 4;
                //     rect.x += 73;
                //     rect.height = 16;
                //     rect.width = 70;
                //     var integerKey = _integerKeys.GetArrayElementAtIndex(i);
                //     integerKey.intValue = EditorGUI.IntField(rect, GUIContent.none, integerKey.intValue);
                //     var ev = Event.current;
                //     if (rect.Contains(ev.mousePosition) && ev.type == EventType.MouseDown && ev.button == 0)
                //     {
                //         Debug.Log("OK");
                //         ev.Use();
                //     }
                // }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                AddItem();
            }

            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }

        private void AddItem()
        {
            (target as EventBranch)?.AddItem();
        }
    }
#endif
}