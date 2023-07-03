#if UNITY_EDITOR
using Common.UnityExtend.Serialization.Tools;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EventSystem
{
    public class EventSystemRuntimeViewerWindow : EditorWindow
    {

        [MenuItem("Tools/EventSystemRuntimeViewerWindow ")]
        public static void Open()
        {
            var window = GetWindow<EventSystemRuntimeViewerWindow>(false, "EventSystemRuntimeViewerWindow", true);
            window.Show();
        }
        private EventSystem _eventSystem;
        private Dictionary<System.Type, List<IEventListener>> _actionListenersDict = null;
        private Vector2 _scrollRect;
        private string _displayText;
        private void TryInit()
        {
            if (_eventSystem == null)
            {
                _eventSystem = EventSystem.Instance;
                var found = typeof(EventSystem).GetField("_actionListenersDict", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(_eventSystem);
                if (found != null)
                {
                    _actionListenersDict = found as Dictionary<System.Type, List<IEventListener>>;
                    RefreshDisplayData();
                }
            }
        }
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            TryInit();
            if (_actionListenersDict == null) return;
            if (GUILayout.Button("Refresh"))
            {
                RefreshDisplayData();
            }
            _scrollRect = EditorGUILayout.BeginScrollView(_scrollRect);
            EditorGUILayout.LabelField(_displayText);
            EditorGUILayout.EndScrollView();
        }
        private void RefreshDisplayData()
        {
            var str = "";
            foreach (var pair in _actionListenersDict)
            {
                var eventName = pair.Key.FullName;
                var list = pair.Value;
                str += $"{eventName} ({list.Count})\n";
            }
            _displayText = str;
        }
    }
}
#endif