#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class UseMemberValueAsLabelAttribute : PropertyAttribute
{
    public string propertyName;
    public UseMemberValueAsLabelAttribute(string propertyName)
    {
        this.propertyName = propertyName;
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(UseMemberValueAsLabelAttribute))]
public class ProductTypeVisualConfigItemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (attribute is not UseMemberValueAsLabelAttribute att) return;
        var propductTypeProp = property.FindPropertyRelative(att.propertyName);
        var lb = propductTypeProp.enumDisplayNames[propductTypeProp.enumValueIndex];
        EditorGUI.PropertyField(position, property, new GUIContent(lb), true);
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
    }
}

#endif