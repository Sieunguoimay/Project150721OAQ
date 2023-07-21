using Common.UnityExtend.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectBinderSO : ScriptableObject
{
#if UNITY_EDITOR
    [StringSelector(nameof(OptionsToDisplay), nameof(DisplayToSerialized), nameof(SerializedToDisplay),true)]
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
        if (_runtimeObject != null)
        {
            Debug.LogError($"ObjectBinderSO:{name}, attempting to bind {target}. _runtimeObject has already been bound to {_runtimeObject}");
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
}