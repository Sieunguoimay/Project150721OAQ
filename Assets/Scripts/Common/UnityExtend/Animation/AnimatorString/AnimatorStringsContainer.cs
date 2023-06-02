using Common.UnityExtend.Animation;
using Common.UnityExtend.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace Common.UnityExtend.Animation
{
    public interface IStringsContainerProvider
    {
        IdentifiedStringsContainer StringContainer { get; }
    }

    public class AnimatorStringsContainer : MonoBehaviour, IStringsContainerProvider
    {
        [SerializeField] private Animator animator;
        [SerializeField] private IdentifiedStringsContainer animatorStringContainer;

        public IdentifiedStringsContainer StringContainer => animatorStringContainer;
        public Animator Animator => animator;

#if UNITY_EDITOR
        public IEnumerable<string> GetAvailableEditorStrings()
        {
            return GetEditorStrings().Where(s => !animatorStringContainer.ContainsValue(s));
        }

        public IEnumerable<string> GetEditorStrings()
        {
            if (animator == null)
            {
                return new string[0];
            }
            var controller = (animator.runtimeAnimatorController as AnimatorController);
            return
                controller.layers.Select(l => l.name)
                .Concat(controller.layers.SelectMany(l => l.stateMachine.states).Select(s => s.state.name))
                .Concat(controller.parameters.Select(p => p.name));
        }
        public IEnumerable<int> GetInvalidItems()
        {
            var strings = GetEditorStrings().ToArray();
            var items = animatorStringContainer.IdentifiedValues;

            for (int i = 0; i < items.Count; i++)
            {
                if (!strings.Contains(items[i].Value))
                {
                    yield return i;
                }
            }
        }

        [ContextMenu("ResetAll")]
        private void ResetAll() => animatorStringContainer.ResetAll();

        [ContextMenu("Automate Fill")]
        public void AutomateFill()
        {
            while (true)
            {
                var found = GetAvailableEditorStrings().FirstOrDefault();
                if (found == null) break;
                animatorStringContainer.CreateNewValue(found);
            }
        }

#endif
    }
}

public class IdentifiedValueContainer<TValue>
{
    [SerializeField] private IdentifiedValue<TValue>[] identifiedValues;
    [SerializeField] private int counter;

    public IReadOnlyList<IdentifiedValue<TValue>> IdentifiedValues => identifiedValues;

    public bool TryGetValue(int localId, out TValue result)
    {
        for (var i = 0; i < identifiedValues.Length; i++)
        {
            if (localId == identifiedValues[i].LocalID)
            {
                result = identifiedValues[i].Value;
                return true;
            }
        }
        result = default;
        return false;
    }
    public bool ContainsValue(TValue value)
    {
        return identifiedValues.Any(v => v.Value.Equals(value));
    }
    public void DeleteEntry(int index)
    {
        if (identifiedValues == null || identifiedValues.Length == 0)
        {
            return;
        }

        int n = identifiedValues.Length;
        for (var i = index; i < n - 1; i++)
        {
            identifiedValues[i] = identifiedValues[i + 1];
        }
        Array.Resize(ref identifiedValues, n - 1);
    }

    public void CreateNewValue(TValue value)
    {
        identifiedValues ??= new IdentifiedValue<TValue>[0];
        int n = identifiedValues.Length;
        Array.Resize(ref identifiedValues, n + 1);
        identifiedValues[n] = IdentifiedValue<TValue>.CreateNew(counter++, value);
    }

    public void ResetAll()
    {
        identifiedValues = new IdentifiedValue<TValue>[0];
        counter = 0;
    }

    public IEnumerable<int> GetInvalidItems()
    {
        if (identifiedValues != null)
        {
            for (var i = 0; i < identifiedValues.Length; i++)
            {
                if (!IsValidItem(i))
                {
                    yield return i;
                }
            }
        }
    }
    bool IsValidItem(int index)
    {
        var itemProp = identifiedValues[index];
        var itemId = itemProp.LocalID;
        var itemValue = itemProp.Value;
        for (var i = 0; i < index; i++)
        {
            var prevItemProp = identifiedValues[i];
            var prevItemValue = prevItemProp.Value;
            var prevItemId = prevItemProp.LocalID;
            if (prevItemValue.Equals(itemValue) || prevItemId.Equals(itemId))
            {
                return false;
            }
        }
        return true;
    }
    [Serializable]
    public class IdentifiedValue<TValue2>
    {
        [SerializeField] private int localID;
        [SerializeField] private TValue2 value;

        public int LocalID => localID;
        public TValue2 Value => value;

        public static IdentifiedValue<TValue2> CreateNew(int localID, TValue2 value)
        {
            return new IdentifiedValue<TValue2> { localID = localID, value = value };
        }
    }
}

[Serializable]
public class IdentifiedStringsContainer : IdentifiedValueContainer<string>
{

}

#if UNITY_EDITOR
[CustomEditor(typeof(AnimatorStringsContainer))]
[CanEditMultipleObjects]
public class AnimatorStringsContainerEditor : Editor
{
    private string[] _stringValueOptions = null;
    private AnimatorStringsContainer _container;
    public override void OnInspectorGUI()
    {
        Initialize();

        var animator = serializedObject.FindProperty("animator");
        var stringContainer = serializedObject.FindProperty("animatorStringContainer");

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(animator);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            ValidateOptions();
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(stringContainer);
        if (EditorGUI.EndChangeCheck())
        {
            Debug.Log("OK");
            ValidateOptions();
        }
        var position = EditorGUILayout.GetControlRect();
        //position.y = position.height;
        //position.x = 50;
        //position.width = 45;
        //position.height = 20;
        DrawAutomateFillButton(position);
    }
    private void Initialize()
    {
        _container ??= target as AnimatorStringsContainer;

        if (_stringValueOptions == null)
        {
            ValidateOptions();
        }
    }
    private void DrawAutomateFillButton(Rect position)
    {
        if (_stringValueOptions == null || _stringValueOptions.Length == 0) return;
        if (GUI.Button(position, $"+ ({_stringValueOptions.Length})"))
        {
            _container.AutomateFill();
            ValidateOptions();
        }
    }
    private void ValidateOptions()
    {
        _stringValueOptions = _container.GetAvailableEditorStrings().ToArray(); ;
    }

}

[CustomPropertyDrawer(typeof(IdentifiedStringsContainer))]
public class IdentifiedStringsContainerDrawer : PropertyDrawer
{
    private List<int> _invalidItems = null;
    private SerializedProperty _identifiedValues;
    private IdentifiedStringsContainer _target;
    private AnimatorStringsContainer _animatorStringsContainer;
    private SerializedProperty FindPropertyValue(SerializedProperty property) => FindProperty(property, "value");
    private SerializedProperty FindPropertyLocalID(SerializedProperty property) => FindProperty(property, "localID");
    private SerializedProperty FindPropertyIdentifiedValues(SerializedProperty property) => FindProperty(property, "identifiedValues");
    private SerializedProperty FindProperty(SerializedProperty property, string propName) => property.FindPropertyRelative(propName);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InitVariables(property);
        position = DrawIdentifiedValuesList(position);
        DrawEditButtons(position);
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        InitVariables(property);
        return (_identifiedValues.arraySize + 1) * (16 + 2);
    }

    private void InitVariables(SerializedProperty property)
    {
        _target ??= SerializeUtility.GetPropertyValue(property) as IdentifiedStringsContainer;
        _identifiedValues ??= FindPropertyIdentifiedValues(property);
        _animatorStringsContainer = _animatorStringsContainer != null ? _animatorStringsContainer : _identifiedValues.serializedObject.targetObject as AnimatorStringsContainer;
        if (_invalidItems == null)
        {
            ValidateItems();
        }
    }
    private void Invalidate()
    {
        _target = null;
        _identifiedValues = null;
        _animatorStringsContainer = null;
        _invalidItems = null;
    }

    private Rect DrawIdentifiedValuesList(Rect position)
    {
        var h = 16;
        position.height = h;
        position.x += 16;
        position.width -= 16 * 2;
        var color = GUI.color;
        for (var i = 0; i < _identifiedValues.arraySize; i++)
        {
            GUI.color = (_invalidItems.Contains(i)) ? Color.red : color;
            DrawIdentifiedValueItem(position, i);
            position.y += h + 2;
        }
        GUI.color = color;
        return position;

    }

    private void ValidateItems()
    {
        _invalidItems = _target.GetInvalidItems().Concat(_animatorStringsContainer.GetInvalidItems()).ToList();
    }

    private void DrawIdentifiedValueItem(Rect position, int i)
    {
        float itemWidth = position.width;
        var identifiedValue = _identifiedValues.GetArrayElementAtIndex(i);

        DrawLocalID(ref position, itemWidth, identifiedValue);
        DrawValue(ref position, itemWidth, identifiedValue);
        DrawDeleteButton(ref position, i, itemWidth);

    }
    void DrawLocalID(ref Rect position, float itemWidth, SerializedProperty identifiedValue)
    {
        var enabled = GUI.enabled;
        GUI.enabled = false;
        position.width = itemWidth / 4 - 16;
        EditorGUI.PropertyField(position, FindPropertyLocalID(identifiedValue), GUIContent.none);
        GUI.enabled = enabled;
    }

    void DrawValue(ref Rect position, float itemWidth, SerializedProperty identifiedValue)
    {
        position.x += itemWidth / 4;
        position.width = itemWidth / 4 * 3 - 8;
        if (EditorGUI.DropdownButton(position, new GUIContent(FindPropertyValue(identifiedValue).stringValue), FocusType.Passive))
        {
            ShowMenuForIdentifiedValueItem(FindPropertyValue(identifiedValue));
        }
    }

    void DrawDeleteButton(ref Rect position, int i, float itemWidth)
    {
        position.x += itemWidth / 4 * 3 - 8 + 4;
        position.width = 20;
        position.height = 16;
        if (GUI.Button(position, "-"))
        {
            _target.DeleteEntry(i);
        }
    }
    private void DrawEditButtons(Rect position)
    {
        position.width = 20;
        position.height = 20;
        DrawAddOneButton(position);
    }
    private void DrawAddOneButton(Rect position)
    {
        if (GUI.Button(position, "+"))
        {
            var stringValueOptions = _animatorStringsContainer.GetAvailableEditorStrings().ToArray();
            _target.CreateNewValue(stringValueOptions.FirstOrDefault() ?? "New Value");
            _identifiedValues.serializedObject.Update();
            ValidateItems();
        }
    }

    private void ShowMenuForIdentifiedValueItem(SerializedProperty property)
    {
        var stringValueOptions = _animatorStringsContainer.GetAvailableEditorStrings().ToArray();
        var menu = new GenericMenu();
        foreach (var option in stringValueOptions)
        {
            menu.AddItem(new GUIContent($"{option}"), property.stringValue.Equals(option), () =>
            {
                property.serializedObject.Update();
                property.stringValue = option;
                property.serializedObject.ApplyModifiedProperties();
                ValidateItems();
            });
        }
        menu.ShowAsContext();
    }
}
#endif
