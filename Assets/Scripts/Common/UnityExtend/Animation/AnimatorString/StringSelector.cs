using Common.UnityExtend.Attribute;
using Common.UnityExtend.Serialization;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Common.UnityExtend.Animation
{
    public class StringSelector : MonoBehaviour
    {
        [SerializeField, TypeConstraint(false, typeof(IStringsContainerProvider))]
        private Object stringContainerProvider;
        [SerializeField, StringSelectorByLocalID(nameof(stringContainerProvider))] private int selectedLocalID;

        private IStringsContainerProvider _stringContainerProvider;
        private IStringsContainerProvider StringContainerProvider => _stringContainerProvider ??= stringContainerProvider as IStringsContainerProvider;
        public string GetValue()
        {
            if (StringContainerProvider.StringContainer.TryGetValue(selectedLocalID, out var result))
            {
                return result;
            }
            return null;
        }
    }

    public class StringSelectorByLocalIDAttribute : PropertyAttribute
    {
        public string providerPropertyName;
        public StringSelectorByLocalIDAttribute(string providerPropertyName)
        {
            this.providerPropertyName = providerPropertyName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(StringSelectorByLocalIDAttribute))]
    public class StringSelectorByLocalIDDrawer : PropertyDrawer
    {
        private IStringsContainerProvider _stringsContainerProvider;
        private GUIContent[] _displayOptions;
        private IReadOnlyList<IdentifiedValueContainer<string>.IdentifiedValue<string>> _options;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_stringsContainerProvider == null || _displayOptions == null)
            {
                _stringsContainerProvider = SerializeUtility.GetSiblingProperty(property, ((StringSelectorByLocalIDAttribute)attribute).providerPropertyName) as IStringsContainerProvider;
                _options = _stringsContainerProvider == null ? new IdentifiedValueContainer<string>.IdentifiedValue<string>[0] : _stringsContainerProvider.StringContainer.IdentifiedValues;
                _displayOptions = _options.Select(v => new GUIContent(v.Value)).ToArray();
            }
            EditorGUI.BeginChangeCheck();
            var current = 0;
            for (; current < _options.Count; current++)
            {
                if (_options[current].LocalID == property.intValue)
                {
                    break;
                }
            }
            var selected = EditorGUI.Popup(position, label, current, _displayOptions);
            if (EditorGUI.EndChangeCheck())
            {

                property.serializedObject.Update();
                property.intValue = _options[selected].LocalID;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}
