using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend.Reflection.Tools
{
    public class RuntimeObjectExposeWindow : EditorWindow
    {
        [MenuItem("Tools/RuntimeObjectExpose")]
        public static void OpenWindow()
        {
            GetWindow(typeof(RuntimeObjectExposeWindow), false, "RuntimeObjectExposeWindow").Show();
        }

        private Object _rootObject;
        private object _currentObject;
        private string _path;

        private IReadOnlyList<RuntimeObjectExpose.ObjectExposedItem> _displayItems;
        private Vector2 _scrollPos;

        private void OnGUI()
        {
            _rootObject = EditorGUILayout.ObjectField(_rootObject, typeof(Object), true);
            _path = EditorGUILayout.TextField(_path);
            if (GUILayout.Button("Expose"))
            {
                UpdateCurrentObject();
                Expose();
            }

            if (_displayItems != null)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true));
                RuntimeObjectExpose.DrawExposedItems(_displayItems);
                EditorGUILayout.EndScrollView();
            }
        }

        private void UpdateCurrentObject()
        {
            if (!string.IsNullOrEmpty(_path))
            {
                var pe = new UnityObjectPathSelector.PathExecutor();
                pe.Setup(_path, _rootObject, false,false);
                _currentObject = pe.ExecutePath();
            }
            else
            {
                _currentObject = _rootObject;
            }
        }

        private void Expose()
        {
            _displayItems = new RuntimeObjectExpose().ExposeObject(_currentObject);
            foreach (var displayItem in _displayItems)
            {
                Debug.Log($"{displayItem.FieldName} {displayItem.DisplayValue}");
            }
        }
    }
}