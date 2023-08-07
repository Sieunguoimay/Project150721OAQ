using Common.UnityExtend.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ObjectBinderSO : ScriptableObject
{
#if UNITY_EDITOR
    [StringSelector(nameof(OptionsToDisplay), nameof(DisplayToSerialized), nameof(SerializedToDisplay), true)]
#endif
    [SerializeField] private string typeConstraint;
    public Type TypeConstraint => string.IsNullOrEmpty(typeConstraint) ? null : Type.GetType(typeConstraint);

#if UNITY_EDITOR
    public IEnumerable<string> OptionsToDisplay => typeof(ObjectBinderSO).Assembly.GetTypes().Select(t => t.FullName);
    public string DisplayToSerialized(string value)
    {
        return Type.GetType(value).AssemblyQualifiedName;
    }
    public string SerializedToDisplay(string value)
    {
        return Type.GetType(value).FullName;
    }
#endif

    private object _runtimeObject = null;

    public void Bind(object target)
    {
        if (!TypeConstraint.IsAssignableFrom(target.GetType()))
        {
            Debug.LogError($"ObjectBinderSO:{name}, attempting to bind instance of type {target.GetType().Name}, required {TypeConstraint.Name}", this);
            return;
        }
        if (_runtimeObject != null)
        {
            Debug.LogError($"ObjectBinderSO:{name}, attempting to bind {target}. _runtimeObject has already been bound to {_runtimeObject}", this);
            return;
        }
        _runtimeObject = target;
    }

    public TObject GetRuntimeObject<TObject>()
    {
        if (_runtimeObject == null)
        {
            Debug.LogError($"ObjectBinderSO: There is no object provided for asset {name}.asset", this);
            return default;
        }
        if (_runtimeObject is TObject) return (TObject)_runtimeObject;

        Debug.LogError($"ObjectBinderSO: Specified type {typeof(TObject)} is invalid. Object type is {_runtimeObject.GetType()}", this);
        return default;
    }

    public class SelectorAttribute : PropertyAttribute
    {
        private readonly Type _constraintType;
        public SelectorAttribute(Type constraintType)
        {
            _constraintType = constraintType;
        }
#if UNITY_EDITOR

        public bool IsMatched(SerializedProperty property)
        {
            if (property.objectReferenceValue == null) return true;
            if (property.objectReferenceValue is ObjectBinderSO binder)
            {
                return binder.TypeConstraint == _constraintType;
            }
            return false;
        }
        public string GetTooltip(SerializedProperty property)
        {
            if (IsMatched(property))
            {
                return _constraintType.FullName;
            }
            if (property.objectReferenceValue is ObjectBinderSO binder)
            {
                return $"Require : {_constraintType.Name}, current: {binder.TypeConstraint.FullName}";
            }
            return "";
        }
#endif
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ObjectBinderSO.SelectorAttribute))]
public class ObjectBinderSelectorAttributeDrawer : PropertyDrawer
{
    private ObjectBinderSO.SelectorAttribute _attribute;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        _attribute ??= attribute as ObjectBinderSO.SelectorAttribute;
        var color = GUI.color;
        GUI.color = _attribute.IsMatched(property) ? color : Color.red;
        label.tooltip = _attribute.GetTooltip(property);
        EditorGUI.PropertyField(position, property, label);
        GUI.color = color;
    }
}
#endif
