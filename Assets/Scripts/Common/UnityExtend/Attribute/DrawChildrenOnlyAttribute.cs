using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UnityExtend.Attribute
{
    public class DrawChildrenOnlyAttribute : PropertyAttribute
    {

    }
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(DrawChildrenOnlyAttribute))]
    public class DrawChildrenOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            //property.
            DrawChildren(position, property);
        }
        private void DrawChildren(Rect position, SerializedProperty property)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var childPosition = new Rect(position.x, position.y, position.width, lineHeight);

            var end = property.GetEndProperty();
            var child = property.Copy();

            child.NextVisible(true);

            while (!SerializedProperty.EqualContents(child, end))
            {
                float childHeight = EditorGUI.GetPropertyHeight(child, GUIContent.none, true);
                EditorGUI.PropertyField(childPosition, child, true);
                childPosition.y += childHeight;
                child.NextVisible(false);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 0f;
            var child = property.Copy();
            var end = property.GetEndProperty();
            child.NextVisible(true);
            while (!SerializedProperty.EqualContents(child, end))
            {
                height += EditorGUI.GetPropertyHeight(child, GUIContent.none, true);
                child.NextVisible(false);
            }

            return height;
        }
    }
#endif
}