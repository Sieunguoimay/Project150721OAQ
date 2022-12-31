using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Common.UnityExtend.Reflection
{
    public interface IEventBranch
    {
        void TriggerByIndex(int index);
        void TriggerIntKey(int key);
        void TriggerStringKey(string key);
        void TriggerBoolKey(bool key);
    }

    public class EventBranch : MonoBehaviour, IEventBranch
    {
        [SerializeField] private BranchKeyType keyType;
        [SerializeField] private string[] stringKeys;
        [SerializeField] private int[] integerKeys;
        [SerializeField] private UnityEvent[] unityEvents;

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

            if ((index < 0 || index > unityEvents.Length))
            {
                AssertInvalidIndex(index);
            }

            unityEvents[index]?.Invoke();
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
            Debug.LogError($"Index {index} is invalid. Events {unityEvents?.Length ?? 0}");
        }

        public bool ValidateBranchKeys()
        {
            var valid = keyType switch
            {
                BranchKeyType.IntegerKey => !integerKeys.GroupBy(x => x).Any(x => x.Count() > 1),
                BranchKeyType.StringKey => !stringKeys.GroupBy(x => x).Any(x => x.Count() > 1),
                _ => true
            };

            return valid;
        }

        [Serializable]
        public enum BranchKeyType
        {
            Index,
            BoolKey,
            IntegerKey,
            StringKey,
        }

        private void OnValidate()
        {
            if (keyType != BranchKeyType.BoolKey) return;
            if (unityEvents == null) unityEvents = new UnityEvent[0];
            if (unityEvents.Length == 2) return;

            Array.Resize(ref unityEvents, 2);
            if (integerKeys.Length < 2)
            {
                Array.Resize(ref integerKeys, 2);
            }

            if (stringKeys.Length < 2)
            {
                Array.Resize(ref stringKeys, 2);
            }
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(EventBranch))]
    public class EventBranchEditor : Editor
    {
        private SerializedProperty _keyType;
        private SerializedProperty _stringKeys;
        private SerializedProperty _integerKeys;
        private SerializedProperty _unityEvents;

        private bool _editKeys;
        private bool _isKeysValid;

        private void OnEnable()
        {
            _keyType = serializedObject.FindProperty("keyType");
            _stringKeys = serializedObject.FindProperty("stringKeys");
            _integerKeys = serializedObject.FindProperty("integerKeys");
            _unityEvents = serializedObject.FindProperty("unityEvents");
            _isKeysValid = (target as EventBranch)?.ValidateBranchKeys() ?? false;
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
            _editKeys = shouldEditKey && DrawEditKeyButton(_editKeys);
            EditorGUILayout.LabelField("Branches", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            var count = _unityEvents.arraySize;
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
                    if (i >= 2) break;
                }
                else if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.IntegerKey)
                {
                    label = $"Key - {_integerKeys.GetArrayElementAtIndex(i).intValue}";
                }
                else if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.StringKey)
                {
                    label = $"Key - {_stringKeys.GetArrayElementAtIndex(i).stringValue}";
                }

                label += " - UnityEvent";

                EditorGUILayout.BeginVertical();
                if (_editKeys)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(15, false);
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                    EditorGUILayout.LabelField("New Key", GUILayout.Width(55));
                    if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.StringKey)
                    {
                        _stringKeys.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(GUIContent.none,
                            _stringKeys.GetArrayElementAtIndex(i).stringValue, GUILayout.Width(50));
                    }
                    else if (_keyType.enumValueIndex == (int) EventBranch.BranchKeyType.IntegerKey)
                    {
                        _integerKeys.GetArrayElementAtIndex(i).intValue = EditorGUILayout.IntField(GUIContent.none,
                            _integerKeys.GetArrayElementAtIndex(i).intValue, GUILayout.Width(50));
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                }

                var rect = EditorGUILayout.BeginHorizontal();
                var unityEvent = _unityEvents.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(unityEvent, new GUIContent(label), true);
                EditorGUILayout.EndHorizontal();

                if (unityEvent.isExpanded && DrawDeleteButtonForEvent(rect))
                {
                    DeleteItem(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            if (_keyType.enumValueIndex != (int) EventBranch.BranchKeyType.BoolKey)
            {
                EditorGUILayout.BeginHorizontal();
                // GUILayout.FlexibleSpace();
                EditorGUILayout.Space(10);
                if (GUILayout.Button("+")) //, GUILayout.Width(20)))
                {
                    AddItem();
                }

                EditorGUILayout.Space(10);

                EditorGUILayout.EndHorizontal();
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                _isKeysValid = (target as EventBranch)?.ValidateBranchKeys() ?? false;
            }

            if (!_isKeysValid)
            {
                var color = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField("Err");
                GUI.color = color;
            }
        }

        private bool DrawEditKeyButton(bool editKey)
        {
            var color = GUI.color;
            GUI.color = _editKeys ? Color.cyan : color;
            if (GUILayout.Button("#", GUILayout.Width(20)))
            {
                editKey = !editKey;
                if (!editKey)
                {
                    GUI.FocusControl(null);
                }
            }

            GUI.color = color;
            return editKey;
        }

        private static bool DrawDeleteButtonForEvent(Rect rect)
        {
            rect.y += rect.height - 16;
            rect.height = 16;
            rect.x = rect.width - 72;
            rect.width = 20;

            var color = GUI.color;
            GUI.color = Color.red;
            var click = GUI.Button(rect, "X");

            GUI.color = color;
            return click;
        }

        private void AddItem()
        {
            var lastIndex = _unityEvents.arraySize;
            _unityEvents.InsertArrayElementAtIndex(lastIndex);
            _integerKeys.InsertArrayElementAtIndex(lastIndex);
            _stringKeys.InsertArrayElementAtIndex(lastIndex);
        }

        private void DeleteItem(int index)
        {
            _unityEvents.DeleteArrayElementAtIndex(index);
            _integerKeys.DeleteArrayElementAtIndex(index);
            _stringKeys.DeleteArrayElementAtIndex(index);
        }
    }
#endif
}