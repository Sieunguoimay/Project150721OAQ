using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend.Attribute
{
    public class ObjectSelectorAttribute : StringSelectorAttribute
    {
        public ObjectSelectorAttribute(string name) : base(name)
        {
        }
    }
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ObjectSelectorAttribute))]
    public class ObjectSelectorDrawer : StringSelectorDrawer
    {
        private IEnumerable<Object> _items;

        protected override IEnumerable<string> GetIds(SerializedProperty property, StringSelectorAttribute objectSelector)
        {
            _items = GetObjects(property, objectSelector);
            return _items.Select(GetName);
        }

        protected virtual string GetName(Object obj)
        {
            return obj != null ? obj.name : null;
        }

        protected virtual IEnumerable<Object> GetObjects(SerializedProperty property, StringSelectorAttribute objectSelector)
        {
            return (objectSelector.GetData(property) as IEnumerable<object> ?? Array.Empty<object>()).Select(o => o as Object);
        }

        protected override bool IsActive(SerializedProperty property, string item)
        {
            return GetName(property.objectReferenceValue)?.Equals(item) ?? false;
        }

        protected override void ModifyProperty(SerializedProperty property, StringSelectorAttribute att, string item)
        {
            property.objectReferenceValue = _items.FirstOrDefault(i => GetName(i).Equals(item));
        }

        protected override bool DrawDropdownButton(Rect position, SerializedProperty property)
        {
            position.width -= 27;
            EditorGUI.ObjectField(position, property, GUIContent.none);
            position.x += position.width + 2;
            position.width = 25;
            return GUI.Button(position, "...");
        }
    }
#endif
}