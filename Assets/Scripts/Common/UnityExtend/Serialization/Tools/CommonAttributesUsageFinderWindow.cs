using Common.DecisionMaking.Actions;
using Common.UnityExtend.Attribute;
using Common.UnityExtend.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using static Common.UnityExtend.Serialization.Tools.CommonAttributesUsageFinder;

namespace Common.UnityExtend.Serialization.Tools
{
    public class CommonEditorWindow
    {
        public static void OpenWindow(Type windowType)
        {
            EditorWindow.GetWindow(windowType, false, windowType.Name, true).Show();
        }
    }

    [EditorWindowTitle(title = "CommonAttributesUsageFinder")]
    public class CommonAttributesUsageFinderWindow : EditorWindow
    {
        [MenuItem("Tools/Snm/Window/" + nameof(CommonAttributesUsageFinderWindow))]
        public static void Open()
        {
            CommonEditorWindow.OpenWindow(typeof(CommonAttributesUsageFinderWindow));
        }

        private AttributeUsageItem[] _attributeUsages;
        private CommonAttributesUsageFinder _finder;

        private void OnGUI()
        {
            DrawCommonAttributesButtons();
            DrawAllAttributeUsageItems();
        }
        private void DrawCommonAttributesButtons()
        {
            var attType = typeof(MenuItem);

            var menuItemAttr = GUILayout.Button(attType.Name);
            if (menuItemAttr)
            {
                FindAttributeUsages(attType);
            }
        }

        private void FindAttributeUsages(Type attType)
        {
            if (_finder == null)
            {
                _finder = new CommonAttributesUsageFinder();
            }
            _attributeUsages = _finder.FindAttributesUsage(attType);
        }
        private void DrawAllAttributeUsageItems()
        {
            if (_attributeUsages == null)
            {
                EditorGUILayout.LabelField("Attribute usages finding result will be shown here..");
                return;
            }

            EditorGUILayout.BeginVertical();
            foreach (var attUsageItem in _attributeUsages)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{attUsageItem.userType.FullName} {string.Join(",", attUsageItem.attributes)}");
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
    public class CommonAttributesUsageFinder
    {
        public CommonAttributesUsageFinder()
        {
        }

        public AttributeUsageItem[] FindAttributesUsage(string attName)
        {
            var _assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return FindAttributesUsage(ReflectionUtility.GetTypeByName(_assemblies, attName));
        }

        public AttributeUsageItem[] FindAttributesUsage(Type attType)
        {
            return FindAttributesUsage(GetType().Assembly, attType);
        }

        public static AttributeUsageItem[] FindAttributesUsage(System.Reflection.Assembly assembly, Type attType)
        {
            var findingResult = new List<AttributeUsageItem>();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var atts = GetCustomAttributes(type, attType);
                if (atts.Length > 0)
                {
                    findingResult.Add(new AttributeUsageItem()
                    {
                        userType = type,
                        attributes = atts,
                    });
                }
            }

            return findingResult.ToArray();
        }
        private static object[] GetCustomAttributes(Type classType, Type attType)
        {
            return classType.GetMembers(ReflectionUtility.PropertyFlags).SelectMany(p => p.GetCustomAttributes(attType, false)).ToArray();
        }

        public class AttributeUsageItem
        {
            public Type userType;
            public Type attributeType;
            public object[] attributes;
        }
    }
}