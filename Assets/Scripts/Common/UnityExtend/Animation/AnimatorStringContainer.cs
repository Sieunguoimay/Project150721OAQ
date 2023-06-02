using Common.UnityExtend.Serialization;
using System;
using UnityEditor;
using UnityEngine;

public class AnimatorStringContainer : MonoBehaviour
{
    [SerializeField] private IdentifiedStringContainer animatorStringContainer;
}

public class IdentifiedValueContainer<TValue>
{
    [SerializeField] private IdentifiedValue<TValue>[] identifiedValues;
    [SerializeField] private int counter;

    public void CreateNewValue(TValue value)
    {
        if (identifiedValues == null)
        {
            identifiedValues = new IdentifiedValue<TValue>[0];
        }
        Array.Resize(ref identifiedValues, identifiedValues.Length + 1);
        identifiedValues[identifiedValues.Length] = IdentifiedValue<TValue>.CreateNew(counter, value);
        counter++;
    }

    [Serializable]
    public class IdentifiedValue<TValue2>
    {
        [SerializeField] private int localID;
        [SerializeField] private TValue2 value;

        public int LocalID => localID;
        public TValue2 Value => value;

        public static IdentifiedValue<TValue2> CreateNew(int localID, TValue2 value)
        {
            return new IdentifiedValue<TValue2> { localID = localID, value = value };
        }
    }
}


[Serializable]
public class IdentifiedStringContainer : IdentifiedValueContainer<string>
{

}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(IdentifiedStringContainer))]
public class IdentifiedStringContainerDrawer : PropertyDrawer
{
    private SerializedProperty identifiedValues;
    private IdentifiedStringContainer _target;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InitVariables(property);
        DrawIdentifiedValues(position);
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        InitVariables(property);
        return identifiedValues.arraySize * (16 + 2);
    }
    private void InitVariables(SerializedProperty property)
    {
        _target ??= SerializeUtility.GetPropertyValue(property) as IdentifiedStringContainer;
        identifiedValues ??= property.FindPropertyRelative(nameof(identifiedValues));
    }
    private void DrawIdentifiedValues(Rect position)
    {
        var h = 16;
        position.height = h;
        position.width /= 2;
        for (var i = 0; i < identifiedValues.arraySize; i++)
        {
            DrawIdentifiedValueItem(position, i);
            position.y += h + 2;
        }
    }

    private void DrawIdentifiedValueItem(Rect position, int i)
    {
        var identifiedValue = identifiedValues.GetArrayElementAtIndex(i);
        var id = identifiedValue.FindPropertyRelative("localID").intValue;
        var value = identifiedValue.FindPropertyRelative("value");
        EditorGUI.LabelField(position, new GUIContent($"{id}"));
        position.x += position.width;
        EditorGUI.PropertyField(position, value, GUIContent.none);
    }
}
#endif
