using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Common.UnityExtend.Serialization
{
    [Serializable]
    public class TypeSelector
    {
        [SerializeField] private string qualifiedName;
        [SerializeField] private string displayName;

        public Type GetSelectedType()
        {
            if (string.IsNullOrEmpty(qualifiedName)) return null;
            return Type.GetType(qualifiedName);
        }

        public class TypeFilterAttribute : PropertyAttribute
        {
            public Type type;
            public TypeFilterAttribute(Type type)
            {
                this.type = type;
            }
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TypeSelector))]
    public class TypeSelectorDrawer : PropertyDrawer
    {
        private SerializedProperty _qualifiedName;
        private SerializedProperty _displayName;
        private SerializedProperty _property;
        private TypeSelector.TypeFilterAttribute _attribute;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _qualifiedName ??= property.FindPropertyRelative("qualifiedName");
            _displayName ??= property.FindPropertyRelative("displayName");
            _property = property;
            _attribute ??= fieldInfo?.GetCustomAttribute<TypeSelector.TypeFilterAttribute>();

            var error = _attribute == null;
            var c = GUI.color;
            GUI.color = error ? Color.red : c;
            var tooltip = error ? "Missing TypeFilterAttribute" : $"Subtype of {_attribute.type.AssemblyQualifiedName}";


            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            var buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

            EditorGUI.PrefixLabel(labelRect, label);

            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(_displayName.stringValue, tooltip), FocusType.Passive))
            {
                ShowMenu();
            }

            EditorGUI.EndProperty();
            GUI.color = c;
        }
        private void SetNewQualifiedName(string qualifiedName,string displayName)
        {
            _qualifiedName.serializedObject.Update();
            _qualifiedName.stringValue = qualifiedName;
            _displayName.stringValue = displayName;
            _qualifiedName.serializedObject.ApplyModifiedProperties();
        }
        private void ShowMenu()
        {
            if (_attribute == null)
            {
                Debug.LogError($"Missing TypeFilterAttribute. Please provide one on this field: {_property.displayName}");
                return;
            }
            var subTypes = GetSubtypes(_attribute.type);//.Select(t => t.AssemblyQualifiedName);
            var menu = new GenericMenu();
            foreach (var st in subTypes)
            {
                var qn = st.AssemblyQualifiedName;
                var fn = st.FullName;
                menu.AddItem(new GUIContent(fn), _qualifiedName.stringValue.Equals(qn), () =>
                {
                    SetNewQualifiedName(qn,fn);
                });
            }
            menu.ShowAsContext();
        }
        private static IEnumerable<Type> GetSubtypes(Type baseType)
        {
            return baseType.Assembly.GetTypes().Where(type => baseType.IsAssignableFrom(type) && type != baseType);
        }
    }
#endif
}