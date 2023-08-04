using System.Collections.Generic;
using Common.UnityExtend.Reflection;
using Common.UnityExtend.Serialization;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class BaseSelectorAttribute : PropertyAttribute
    {
        private readonly string _optionProvide;
        private readonly string _displayToSerialized;
        private readonly string _serializedToDisplay;

        protected BaseSelectorAttribute(string name, string callbackToModifySelectedValue, string callbackToModifyDisplayValue = "")
        {
            _optionProvide = name;
            _displayToSerialized = callbackToModifySelectedValue;
            _serializedToDisplay = callbackToModifyDisplayValue;
        }
#if UNITY_EDITOR

        public object GetData(SerializedProperty property)
        {
            var providerObject = SerializeUtility.GetObjectToWhichPropertyBelong(property);
            var result = ReflectionUtility.GetDataFromMember(providerObject, _optionProvide, false);
            if (result == null)
            {
                providerObject = property.serializedObject.targetObject;
                result = ReflectionUtility.GetDataFromMember(providerObject, _optionProvide, false);
            }
            return result;
        }

        public object GetSerializedValue(SerializedProperty property, object displayValue)
        {
            if (string.IsNullOrEmpty(_displayToSerialized)) return displayValue;
            var providerObject = SerializeUtility.GetObjectToWhichPropertyBelong(property);
            return providerObject.GetType().GetMethod(_displayToSerialized, ReflectionUtility.MethodFlags)
                ?.Invoke(providerObject, new[] { displayValue });
        }

        public object GetDisplayValue(SerializedProperty property, object serializedValue)
        {
            if (string.IsNullOrEmpty(_serializedToDisplay)) return serializedValue;
            var providerObject = SerializeUtility.GetObjectToWhichPropertyBelong(property);
            return providerObject.GetType().GetMethod(_serializedToDisplay, ReflectionUtility.MethodFlags)
                ?.Invoke(providerObject, new[] { serializedValue });
        }
        // public void InvokeCallback(SerializedProperty property)
        // {
        //     if (string.IsNullOrEmpty(_callbackMethod)) return;
        //     var providerObject = _isCallbackInBase
        //         ? property.serializedObject.targetObject
        //         : ReflectionUtility.GetObjectToWhichPropertyBelong(property);
        //     ReflectionUtility.GetMethodInfo(providerObject.GetType(), _callbackMethod, false).Invoke(providerObject, null);
        // }
#endif
    }

    public class StringSelectorAttribute : BaseSelectorAttribute
    {
        public bool UseSearchMenu { get; private set; } = false;
        public StringSelectorAttribute(string name, string callbackToModifySelectedValue = "", string callbackToModifyDisplayValue = "", bool useSearchMenu = false)
            : base(name, callbackToModifySelectedValue, callbackToModifyDisplayValue)
        {
            UseSearchMenu = useSearchMenu;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(StringSelectorAttribute))]
    public class StringSelectorDrawer : PropertyDrawer
    {
        private GenericMenu _menu;
        private SearchMenuWindow _searchMenu;
        private StringSelectorAttribute _objectSelector;
        private string _cachedString;
        private string _displayString;
        private SerializedProperty _property;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Draw(position, property, label, EditorStyles.label);
        }

        protected void Draw(Rect position, SerializedProperty property, GUIContent label, GUIStyle style)
        {
            _property ??= property;
            _objectSelector ??= attribute as StringSelectorAttribute;
            if (string.IsNullOrEmpty(_cachedString) && !string.IsNullOrEmpty(property.stringValue))
            {
                UpdateString(property);
            }

            position = EditorGUI.PrefixLabel(position, label, style);
            var openMenu = DrawDropdownButton(position, property);

            if (openMenu)
            {
                ShowMenu();
            }
        }
        private void ShowMenu()
        {
            var ids = GetIds(_property, _objectSelector);
            if (ids == null) return;

            if (_objectSelector.UseSearchMenu)
            {
                ShowSearchMenuWithStringProperty(ids);
            }
            else
            {
                ShowMenuWithStringProperty(ids);
            }
        }

        private void ShowSearchMenuWithStringProperty(IEnumerable<string> ids)
        {
            _searchMenu = SearchMenuWindow.Create(_property.serializedObject.targetObject.name, true);
            foreach (var id in ids)
            {
                _searchMenu.AddItem(id, data =>
                {
                    OnMenuItemSelected((string)data);
                }, id);
            }
            _searchMenu.ShowMenu();
        }
        private void ShowMenuWithStringProperty(IEnumerable<string> ids)
        {
            _menu = new GenericMenu();
            foreach (var id in ids)
            {
                _menu.AddItem(new GUIContent(id), IsActive(_property, id), data =>
                {
                    OnMenuItemSelected((string)data);
                }, id);
            }
            _menu.ShowAsContext();
        }

        private void OnMenuItemSelected(string str)
        {
            _property.serializedObject.Update();
            ModifyProperty(_property, _objectSelector, str);
            _property.serializedObject.ApplyModifiedProperties();
            UpdateString(_property);
        }

        protected virtual bool DrawDropdownButton(Rect position, SerializedProperty property)
        {
            return EditorGUI.DropdownButton(position, new GUIContent(_displayString), FocusType.Keyboard);
        }

        private void UpdateString(SerializedProperty property)
        {
            if (_cachedString != property.stringValue)
            {
                _cachedString = property.stringValue;
                _displayString = (string)_objectSelector.GetDisplayValue(property, property.stringValue);
            }
        }

        protected virtual bool IsActive(SerializedProperty property, string item)
        {
            return property.stringValue == item;
        }

        protected virtual void ModifyProperty(SerializedProperty property, StringSelectorAttribute att, string item)
        {
            property.stringValue = (string)att.GetSerializedValue(property, item);
        }

        protected virtual IEnumerable<string> GetIds(SerializedProperty property,
            StringSelectorAttribute objectSelector)
        {
            return objectSelector.GetData(property) as IEnumerable<string>;
        }
    }
#endif
}