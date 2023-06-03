using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class TypeConstraintAttribute : PropertyAttribute
    {
        public Type[] RequiredTypes { get; }

        public bool ShowConstraintType { get; }

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

        public TypeConstraintAttribute(bool showConstraintType, params Type[] requiredTypes)
        {
            ShowConstraintType = showConstraintType;
            RequiredTypes = requiredTypes;
        }

        public TypeConstraintAttribute(params Type[] requiredTypes)
        {
            ShowConstraintType = true;
            RequiredTypes = requiredTypes;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TypeConstraintAttribute), true)]
    public class TypeConstraintPropertyDrawer : PropertyDrawer
    {
        private static void Draw(TypeConstraintAttribute attr, Rect position, SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();

            // Show property field. Tint it if the last attempted assignment was invalid
            var invalid = property.objectReferenceValue != null && attr.RequiredTypes.Any(constrainedType =>
                !constrainedType.IsInstanceOfType(property.objectReferenceValue));

            var prevGuiColor = GUI.color;
            GUI.color = invalid ? Color.red : prevGuiColor;

            var width = position.width - 27;
            position.width = width;
            var label = attr.ShowConstraintType
                ? new GUIContent($"{property.displayName} ({attr.RequiredTypesLabel})")
                : new GUIContent($"{property.displayName}", $"{attr.RequiredTypesLabel}");
            EditorGUI.PropertyField(position, property, label);

            GUI.color = prevGuiColor;

            position.x += width + 2;
            position.width = 25;
            Menu(position, attr, property, true);
        }
        private static UnityEngine.Object[] ExtractObjects(TypeConstraintAttribute typeConstraint, UnityEngine.Object go)
        {
            if (go is GameObject || go is Component)
            {
                if (go is GameObject g)
                {
                    var assets = g.GetComponentsInChildren<Component>();
                    return assets.Where(ass => typeConstraint.RequiredTypes.Any(rt => rt.IsInstanceOfType(ass))).ToArray();
                }
                else if (go is Component c)
                {
                    var assets = c.gameObject.GetComponentsInChildren<Component>();
                    return assets.Where(ass => typeConstraint.RequiredTypes.Any(rt => ass.GetType().IsInstanceOfType(rt))).ToArray();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(go);
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                return assets.Where(ass => typeConstraint.RequiredTypes.Any(rt => ass.GetType().IsInstanceOfType(rt))).ToArray();
            }
        }

        public static void Menu(Rect rect, TypeConstraintAttribute attr, SerializedProperty property, bool showIndex)
        {

            var prevGuiColor = GUI.color;
            GUI.color = Color.cyan;
            var show = GUI.Button(rect, "...");
            GUI.color = prevGuiColor;

            if (show)
            {
                var objectToExtract = property.objectReferenceValue
                    ? property.objectReferenceValue
                    : property.serializedObject.targetObject;

                var candidates = ExtractObjects(attr, objectToExtract);
                CreateMenu(candidates, property, showIndex);
            }
        }
        private static void CreateMenu(UnityEngine.Object[] candidates, SerializedProperty property, bool showIndex)
        {
            var menu = new GenericMenu();
            for (var i = 0; i < candidates.Length; i++)
            {
                var c = candidates[i];
                var text = showIndex
                    ? $"{c.GetType().Name} {(candidates.Length > 1 ? i + 1 : "")}"
                    : c.GetType().Name;
                menu.AddItem(new GUIContent(text), property.objectReferenceValue == c, () =>
                {
                    property.objectReferenceValue = c;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Draw(attribute as TypeConstraintAttribute, position, property);
        }
    }
#endif
}