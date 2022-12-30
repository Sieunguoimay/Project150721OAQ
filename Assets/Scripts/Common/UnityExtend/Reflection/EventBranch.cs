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

        private bool _editKeys;

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

            var shouldEditKey =
                _keyType.enumValueIndex is (int) EventBranch.BranchKeyType.IntegerKey or (int) EventBranch.BranchKeyType
                    .StringKey;

            EditorGUILayout.BeginHorizontal();
            if (shouldEditKey)
            {
                var color = GUI.color;
                GUI.color = _editKeys ? Color.cyan : color;
                if (GUILayout.Button("#", GUILayout.Width(20)))
                {
                    _editKeys = !_editKeys;
                }

                GUI.color = color;
            }

            EditorGUILayout.LabelField("Branches", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            var count = _eventHandlerGroups.arraySize;
            var label = "None";
            for (var i = 0; i < count; i++)
            {
                if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.Index)
                {
                    label = ($"Index - {i}");
                }
                else if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.BoolKey)
                {
                    label = i == 1 ? "True" : "False";
                    if (i > 2) continue;
                }
                else if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.IntegerKey)
                {
                    label = $"Key - {_integerKeys.GetArrayElementAtIndex(i).intValue}";
                }
                else if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.StringKey)
                {
                    label = $"Key - {_stringKeys.GetArrayElementAtIndex(i).stringValue}";
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (_editKeys)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(15, false);
                    EditorGUILayout.LabelField("New Key", GUILayout.Width(55));
                    _stringKeys.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(GUIContent.none,
                        _stringKeys.GetArrayElementAtIndex(i).stringValue, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal(); //EditorStyles.helpBox);
                EditorGUILayout.Space(15, false);
                var eventHandlers =
                    _eventHandlerGroups.GetArrayElementAtIndex(i)
                        .FindPropertyRelative(nameof(EventBranch.EventHandlerGroup.eventHandlers));

                if (EditorGUILayout.PropertyField(eventHandlers, new GUIContent(label), true))
                {
                    Debug.Log("OK");
                }

                EditorGUILayout.Space(2, false);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
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