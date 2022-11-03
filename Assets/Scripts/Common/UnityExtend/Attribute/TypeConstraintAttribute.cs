using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    /// <summary>
    /// Constrains a property to a range of Types.
    /// </summary>
    public class TypeConstraintAttribute : PropertyAttribute
    {
        /// <summary>
        /// The Types this property must derive from.
        /// </summary>
        public Type[] RequiredTypes { get; }

        /// <summary>
        /// A readable form of the required Types.
        /// </summary>
        public string RequiredTypesLabel
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var requiredType in RequiredTypes)
                {
                    if (sb.Length > 0)
                        sb.Append(" && ");

                    sb.Append(requiredType.Name);
                }

                return sb.ToString();
            }
        }

        public TypeConstraintAttribute(params Type[] requiredTypes) => RequiredTypes = requiredTypes;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TypeConstraintAttribute), true)]
    public class TypeConstraintPropertyDrawer : PropertyDrawer
    {
        private static void Draw(TypeConstraintAttribute typeConstraint, Rect position, SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();

            // Show property field. Tint it if the last attempted assignment was invalid
            var prevGuiColor = GUI.color;
            if (property.objectReferenceValue != null && typeConstraint.RequiredTypes.Any(constrainedType => !constrainedType.IsInstanceOfType(property.objectReferenceValue)))
                GUI.color = Color.red;

            position.width -= 30;
            EditorGUI.PropertyField(position, property, new GUIContent($"{property.displayName} ({typeConstraint.RequiredTypesLabel})"));

            GUI.color = prevGuiColor;

            var btnRect = position;
            btnRect.x += position.width + 2;
            btnRect.width = 25;
            Menu(btnRect, property, typeConstraint.RequiredTypes);

        }

        private static void Menu(Rect rect, SerializedProperty property, Type[] types)
        {
            var value = property.objectReferenceValue ? property.objectReferenceValue : property.serializedObject.targetObject;

            var go = value as GameObject ?? (value as Component)?.gameObject;

            if (go != null)
            {
                var prevGuiColor = GUI.color;
                GUI.color = Color.cyan;
                var show = GUI.Button(rect, "...");
                GUI.color = prevGuiColor;

                if (!show)
                {
                    return;
                }

                var candidates = types.SelectMany(i => go.GetComponentsInChildren(i, true)).Distinct().ToArray();
                var menu = new GenericMenu();
                for (var i = 0; i < candidates.Length; i++)
                {
                    var c = candidates[i];
                    menu.AddItem(new GUIContent($"{c.GetType().Name} {(candidates.Length > 1 ? i + 1 : "")}"), value == c, () =>
                    {
                        property.objectReferenceValue = c;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            }
            else
            {
                var prevGuiColor = GUI.color;
                GUI.color = Color.grey;
                GUI.Button(rect, "...");
                GUI.color = prevGuiColor;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Draw(attribute as TypeConstraintAttribute, position, property);
        }
    }
#endif
}