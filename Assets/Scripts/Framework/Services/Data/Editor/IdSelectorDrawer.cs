using System.Collections.Generic;
using Common.UnityExtend.Attribute;
using UnityEditor;
using UnityEngine;

namespace Framework.Services.Data.Editor
{
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(DataAssetIdSelectorAttribute))]
    public class IdSelectorDrawer : StringSelectorDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var clickArea = position;
            Draw(position, property, new GUIContent(label.text, "Click Notify Asset"), new GUIStyle(GUI.skin.box) {alignment = TextAnchor.MiddleLeft});
            var current = Event.current;
            if (clickArea.Contains(current.mousePosition) && current.type == EventType.MouseDown && current.button == 0)
            {
                if (!string.IsNullOrEmpty(property.stringValue))
                {
                    EditorGUIUtility.PingObject(IdsHelper.GetDataAssetById(property.stringValue));
                }

                current.Use();
            }
        }

        protected override IEnumerable<string> GetIds(SerializedProperty property, StringSelectorAttribute objectSelector)
        {
            return IdsHelper.GetIds((objectSelector as DataAssetIdSelectorAttribute)?.TypeConstraint);
        }
    }
#endif
}