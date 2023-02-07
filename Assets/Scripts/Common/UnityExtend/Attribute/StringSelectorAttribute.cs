using System.Collections.Generic;
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
        private readonly string _callbackToModifySelectedValue;

        protected BaseSelectorAttribute(string name, bool isProviderPropertyInBase, string callbackToModifySelectedValue)
        {
            _providerVariableName = name;
            _isProviderPropertyInBase = isProviderPropertyInBase;
            _callbackToModifySelectedValue = callbackToModifySelectedValue;
        }

        public object GetData(SerializedProperty property)
        {
            var providerObject = _isProviderPropertyInBase
                ? property.serializedObject.targetObject
                : SerializeUtility.GetObjectToWhichPropertyBelong(property);
            return ReflectionUtility.GetDataFromMember(providerObject, _providerVariableName, false);
        }

        public object InvokeCallback(SerializedProperty property, object value)
        {
            if (string.IsNullOrEmpty(_callbackToModifySelectedValue)) return value;
            var providerObject = SerializeUtility.GetObjectToWhichPropertyBelong(property);
            return providerObject.GetType().GetMethod(_callbackToModifySelectedValue)?.Invoke(providerObject, new[] {value});
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
        public StringSelectorAttribute(string name, bool isProviderPropertyInBase = false, string callbackToModifySelectedValue = "")
            : base(name, isProviderPropertyInBase, callbackToModifySelectedValue)
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
            Draw(position, property, label, EditorStyles.label);
        }

        protected void Draw(Rect position, SerializedProperty property, GUIContent label, GUIStyle style)
        {
            if (attribute is not StringSelectorAttribute objectSelector) return;
            position = EditorGUI.PrefixLabel(position, label, style);
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
                    OnSelected(property, objectSelector, (string) data);
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

        protected virtual void OnSelected(SerializedProperty property, StringSelectorAttribute att, string item)
        {
            property.stringValue = (string) att.InvokeCallback(property, item);
        }

        protected virtual IEnumerable<string> GetIds(SerializedProperty property,
            StringSelectorAttribute objectSelector)
        {
            return objectSelector.GetData(property) as IEnumerable<string>;
        }
    }
#endif
}