using System;
using System.Collections.Generic;
using Common.UnityExtend.Reflection;
using Common.UnityExtend.Serialization;
using Common.UnityExtend.Serialization.ChildAsset;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class ChildAssetAttribute : PropertyAttribute
    {
        public ChildAssetAttribute(bool drawCreateChildAsset = true)
        {
            DrawCreateChildAsset = drawCreateChildAsset;
        }

        [field: System.NonSerialized] public bool DrawCreateChildAsset { get; }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ChildAssetAttribute))]
    public class ChildAssetAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var drawCreateChildAsset = (attribute as ChildAssetAttribute)?.DrawCreateChildAsset ?? false;
            position.width -= 44 / (float)(drawCreateChildAsset ? 1 : 2);
            EditorGUI.PropertyField(position, property, label);

            if (drawCreateChildAsset)
            {
                position.x += position.width + 2;
                position.width = 20;

                if (GUI.Button(position, "..."))
                {
                    ShowMenu(new (string, GenericMenu.MenuFunction)[]
                    {
                        ("Create child asset", () => CreateChildAsset(property)),
                    });
                }
            }

            position.x += position.width + 2;
            position.width = 20;

            if (GUI.Button(position, "~"))
            {
                Debug.Log("Open window");
                ChildAssetManagerWindow.Open(); //property.serializedObject.targetObject);
            }
        }

        private static void CreateChildAsset(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference) return;

            var type = SerializeUtility.GetSiblingPropertyType(property, property.name);
            var instance = ScriptableObject.CreateInstance(type);
            instance.name = type.Name;

            AssetDatabase.AddObjectToAsset(instance,
                AssetDatabase.GetAssetPath(property.serializedObject.targetObject));

            property.serializedObject.Update();
            property.objectReferenceValue = instance;
            property.serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ShowMenu(IEnumerable<(string, GenericMenu.MenuFunction)> menuItems)
        {
            var menu = new GenericMenu();
            foreach (var (name, menuFunction) in menuItems)
            {
                menu.AddItem(new GUIContent(name), false, menuFunction);
            }

            menu.ShowAsContext();
        }
    }
#endif
}