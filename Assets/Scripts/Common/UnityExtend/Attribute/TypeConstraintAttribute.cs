using System;
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

        public bool ShowConstraintType { get; }

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
        private static void Draw(TypeConstraintAttribute typeConstraint, Rect position, SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();

            // Show property field. Tint it if the last attempted assignment was invalid
            var prevGuiColor = GUI.color;
            if (property.objectReferenceValue != null && typeConstraint.RequiredTypes.Any(constrainedType =>
                !constrainedType.IsInstanceOfType(property.objectReferenceValue)))
                GUI.color = Color.red;

            var width = position.width - 27;

            position.width = width;
            EditorGUI.PropertyField(position, property,
                new GUIContent(typeConstraint.ShowConstraintType
                    ? $"{property.displayName} ({typeConstraint.RequiredTypesLabel})"
                    : $"{property.displayName}"));

            GUI.color = prevGuiColor;

            position.x += width + 2;
            position.width = 25;
            Menu(position, property,
                go =>
                {
                    if ((go is GameObject || go is Component) && !AssetDatabase.IsMainAsset(go))
                    {
                        if (go is GameObject g)
                        {
                            var assets = g.GetComponentsInChildren<Component>();
                            return assets.Where(ass => typeConstraint.RequiredTypes.Any(rt => ass.GetType().IsAssignableFrom(rt))).ToArray();
                        }
                        else
                        if (go is Component c)
                        {
                            var assets = c.gameObject.GetComponentsInChildren<Component>();
                            return assets.Where(ass => typeConstraint.RequiredTypes.Any(rt => ass.GetType().IsAssignableFrom(rt))).ToArray();
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
                        return assets.Where(ass => typeConstraint.RequiredTypes.Any(rt => ass.GetType().IsAssignableFrom(rt))).ToArray();
                    }
                }, true);
        }

        public static void Menu(Rect rect, SerializedProperty property, Func<UnityEngine.Object, UnityEngine.Object[]> filter,
            bool showIndex)
        {
            var unityObject = property.objectReferenceValue
                ? property.objectReferenceValue
                : property.serializedObject.targetObject;

            //var go = value as GameObject ?? (value as Component)?.gameObject;

            if (unityObject != null)
            {
                var prevGuiColor = GUI.color;
                GUI.color = Color.cyan;
                var show = GUI.Button(rect, "...");
                GUI.color = prevGuiColor;

                if (!show)
                {
                    return;
                }

                var candidates = filter.Invoke(unityObject);
                var menu = new GenericMenu();
                for (var i = 0; i < candidates.Length; i++)
                {
                    var c = candidates[i];
                    var text = showIndex
                        ? $"{c.GetType().Name} {(candidates.Length > 1 ? i + 1 : "")}"
                        : c.GetType().Name;
                    menu.AddItem(new GUIContent(text), unityObject == c, () =>
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