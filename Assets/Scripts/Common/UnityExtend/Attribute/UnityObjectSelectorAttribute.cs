using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class UnityObjectSelectorAttribute : PropertyAttribute
    {
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(UnityObjectSelectorAttribute))]
    public class UnityObjectSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // base.OnGUI(position, property, label);
            var fullWidth = position.width;
            position.width = fullWidth - 27;
            EditorGUI.PropertyField(position, property, label);

            var btnRect = position;
            btnRect.x = position.x + fullWidth - 25;
            btnRect.width = 25;
            TypeConstraintPropertyDrawer.Menu(btnRect, property, go => go.GetComponents<Component>().Concat(new[] {go as Object}).ToArray(), false);
        }
    }
#endif
}