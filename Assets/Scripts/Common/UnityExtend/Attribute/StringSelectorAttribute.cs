using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.UnityExtend.Reflection;
using Common.UnityExtend.Serialization;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class BaseSelectorAttribute : PropertyAttribute
    {
        private readonly string _providerVariableName;
        private readonly bool _isProviderPropertyInBase;

        protected BaseSelectorAttribute(string name, bool isProviderPropertyInBase)
        {
            _providerVariableName = name;
            _isProviderPropertyInBase = isProviderPropertyInBase;
        }

        public object GetData(SerializedProperty property)
        {
            var providerObject = _isProviderPropertyInBase
                ? property.serializedObject.targetObject
                : ReflectionUtility.GetObjectToWhichPropertyBelong(property);
            return ReflectionUtility.GetDataFromMember(providerObject, _providerVariableName, false);
        }

        // public void InvokeCallback(SerializedProperty property)
        // {
        //     if (string.IsNullOrEmpty(_callbackMethod)) return;
        //     var providerObject = _isCallbackInBase
        //         ? property.serializedObject.targetObject
        //         : ReflectionUtility.GetObjectToWhichPropertyBelong(property);
        //     ReflectionUtility.GetMethodInfo(providerObject.GetType(), _callbackMethod, false).Invoke(providerObject, null);
        // }
    }

    public class StringSelectorAttribute : BaseSelectorAttribute
    {
        public StringSelectorAttribute(string name, bool isProviderPropertyInBase = false)
            : base(name, isProviderPropertyInBase)
        {
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(StringSelectorAttribute))]
    public class StringSelectorDrawer : PropertyDrawer
    {
        private GenericMenu _menu;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not StringSelectorAttribute objectSelector) return;
            position = EditorGUI.PrefixLabel(position, label);

            if (!CreateMenuWithStringProperty(position, property, objectSelector)) return;

            _menu?.ShowAsContext();
        }

        private bool CreateMenuWithStringProperty(Rect position, SerializedProperty property,
            StringSelectorAttribute objectSelector)
        {
            var openWindow = DrawDropdownButton(position, property);
            if (!openWindow)
            {
                _menu = null;
                return false;
            }

            if (_menu != null) return true;
            _menu = new GenericMenu();

            var ids = GetIds(property, objectSelector);

            if (ids == null) return false;

            foreach (var id in ids)
            {
                _menu.AddItem(new GUIContent(id), IsActive(property, id), data =>
                {
                    property.serializedObject.Update();
                    OnSelected(property, (string) data);
                    property.serializedObject.ApplyModifiedProperties();
                }, id);
            }

            return true;
        }

        protected virtual bool DrawDropdownButton(Rect position, SerializedProperty property)
        {
            return EditorGUI.DropdownButton(position, new GUIContent(GetDisplay(property)), FocusType.Keyboard);
        }

        protected virtual string GetDisplay(SerializedProperty property)
        {
            return property.stringValue;
        }

        protected virtual bool IsActive(SerializedProperty property, string item)
        {
            return property.stringValue == item;
        }

        protected virtual void OnSelected(SerializedProperty property, string item)
        {
            property.stringValue = item;
        }

        protected virtual IEnumerable<string> GetIds(SerializedProperty property,
            StringSelectorAttribute objectSelector)
        {
            return objectSelector.GetData(property) as IEnumerable<string>;
        }
    }
#endif
}