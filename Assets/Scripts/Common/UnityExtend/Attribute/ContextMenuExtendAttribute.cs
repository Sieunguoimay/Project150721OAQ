using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.UnityExtend.Reflection;
using UnityEditor;
using UnityEngine;

namespace Common.UnityExtend.Attribute
{
    public class ContextMenuExtendAttribute : System.Attribute
    {
        public ContextMenuExtendAttribute(string menuItem)
        {
            MenuItem = menuItem;
        }

        public string MenuItem { get; }
    }

#if UNITY_EDITOR
    ///<summary>
    ///Override this to provide custom context menu for [ContextMenuExtend]
    ///</summary>
    public abstract class BaseContextMenuExtendEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawClickArea(targets, target.GetType().Name, target);
            base.OnInspectorGUI();
        }

        public static void DrawClickArea(Object[] targets, string displayName, Object active)
        {
            var clickArea = EditorGUILayout.BeginVertical(GUI.skin.box);
            var skin = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.LowerCenter, fontSize = 20};
            EditorGUILayout.LabelField(displayName, skin, GUILayout.Height(30));
            skin.alignment = TextAnchor.UpperCenter;
            skin.fontSize = 10;
            EditorGUILayout.LabelField("(Custom Context Menu Area)", skin, GUILayout.Height(20));
            EditorGUILayout.EndVertical();

            var current = Event.current;

            if (clickArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
            {
                ShowContextMenuExtend(targets);
            }

            if (clickArea.Contains(current.mousePosition) && current.type == EventType.MouseDown && current.button == 0)
            {
                EditorGUIUtility.PingObject(active);
                current.Use();
            }
        }

        public static void ShowContextMenuExtend(Object[] targets)
        {
            if (!targets.Any()) return;
            var thisType = targets.First().GetType();
            var items =
                ReflectionUtility.GetAllMethods(thisType).SelectMany(m =>
                    m.GetCustomAttributes(typeof(ContextMenu), true).Select(a => (m, (a as ContextMenu)?.menuItem))
                        .Concat(m.GetCustomAttributes(typeof(ContextMenuExtendAttribute), true).Select(c => (m, (c as ContextMenuExtendAttribute)?.MenuItem))));
            var menu = new GenericMenu();
            foreach (var (m, n) in items)
            {
                menu.AddItem(new GUIContent(n), false, () =>
                {
                    foreach (var t in targets)
                    {
                        m.Invoke(t, null);
                    }
                });
            }

            menu.ShowAsContext();
        }
    }
#endif
}