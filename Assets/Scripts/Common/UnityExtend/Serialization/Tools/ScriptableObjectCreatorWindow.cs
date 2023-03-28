using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.UnityExtend.Serialization.Tools
{
    [EditorWindowTitle(title = "ScriptableObject Assets")]
    public class ScriptableObjectCreatorWindow : EditorWindow
    {
        [MenuItem("Tools/ScriptableObject Assets")]
        public static void Open()
        {
            var window = GetWindow<ScriptableObjectCreatorWindow>("ScriptableObject Assets");
            window.Init();
            window.Show();
        }
        private void OnEnable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            Repaint();
        }

        private string _searchName;

        private MonoScript[] _allMonoScripts;
        private MonoScript[] _searchMatchedScripts;

        private Vector2 _scrollPosition;
        private MonoScript _targetMonoScript;
        private Object[] _instanceAssetsOfTargetMonoScript;

        private static GUIStyle _searchIconStyle;
        private static GUIStyle _textFieldStyle;
        private static GUIStyle _horizontalLayoutStyle;
        private static GUIStyle _createBtnStyle;
        private static GUIContent _searchIcon;

        private static string _selectedPath;
        private static Object _globalSelectedObject;


        private static void InitStyles()
        {
            _searchIconStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(2, 0, 0, 0)
            };
            _textFieldStyle = new GUIStyle
            {
                normal = EditorStyles.textField.normal,
                margin = new RectOffset(),
                padding = new RectOffset()
            };
            _horizontalLayoutStyle = new GUIStyle(EditorStyles.textField)
            {
                padding = new RectOffset()
            };
            _createBtnStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft
            };
            _searchIcon = EditorGUIUtility.IconContent("Search Icon");
        }

        private void Init()
        {
            var textAssetPaths = AssetDatabase.FindAssets("t:MonoScript", new[] {"Assets"});
            _allMonoScripts = textAssetPaths
                .Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<TextAsset>)
                .OfType<MonoScript>().Where(IsMonoScriptOfScriptableObject).ToArray();
            _searchMatchedScripts = _allMonoScripts;
            _searchName = "";
        }

        private void InitIfNeeded()
        {
            if (_allMonoScripts == null)
            {
                Init();
            }

            if (_horizontalLayoutStyle == null)
            {
                InitStyles();
            }
        }

        private void OnGUI()
        {
            InitIfNeeded();

            EditorGUILayout.BeginVertical();

            DrawSearchField();

            DrawSelectedPath();

            DrawMatchedTypes();

            EditorGUILayout.EndVertical();
        }

        private void DrawSearchField()
        {
            EditorGUILayout.BeginHorizontal(_horizontalLayoutStyle);
            GUILayout.Label(_searchIcon, _searchIconStyle, GUILayout.Width(12), GUILayout.Height(16));
            var newString = EditorGUILayout.TextField(_searchName, _textFieldStyle);
            EditorGUILayout.EndHorizontal();

            if (_searchName != newString)
            {
                _searchName = newString;
                UpdateOptionList();
            }
        }

        private static void DrawSelectedPath()
        {
            var selected = Selection.activeObject;
            if (_globalSelectedObject != selected)
            {
                _globalSelectedObject = selected;

                _selectedPath = AssetDatabase.GetAssetPath(_globalSelectedObject);
                if (string.IsNullOrEmpty(_selectedPath))
                {
                    _selectedPath = "Assets";
                }
            }

            EditorGUILayout.LabelField(new GUIContent("Create Path: " + _selectedPath, "Save path"));
        }

        private void DrawMatchedTypes()
        {
            if (_searchMatchedScripts == null) return;
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (var monoScript in _searchMatchedScripts)
            {
                DrawMonoScript(monoScript);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawMonoScript(MonoScript monoScript)
        {
            EditorGUILayout.BeginHorizontal();

            DrawMonoScriptObjectField(monoScript);

            DrawCreateButton(monoScript);

            DrawFindButton(monoScript);

            EditorGUILayout.EndHorizontal();

            DrawInstanceAssetsOfTarget(monoScript);
        }

        private static void DrawMonoScriptObjectField(MonoScript monoScript)
        {
            var ge = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.ObjectField(monoScript, typeof(MonoScript), false);
            GUI.enabled = ge;
        }

        private static void DrawCreateButton(MonoScript monoScript)
        {
            var tooltip = "Create asset at path: " + _selectedPath;
            var createBtnContent = new GUIContent("+", tooltip);
            if (GUILayout.Button(createBtnContent, _createBtnStyle, GUILayout.Width(20)))
            {
                CreateAsset(monoScript);
            }
        }

        private void DrawFindButton(MonoScript monoScript)
        {
            var globalColor = GUI.backgroundColor;
            var active = _targetMonoScript == monoScript;
            GUI.backgroundColor = active ? Color.gray : globalColor;

            var findBtnContent = new GUIContent(_searchIcon.image, "Find Instance Assets");

            if (GUILayout.Button(findBtnContent,new GUIStyle(GUI.skin.button){padding = new RectOffset(2,2,2,2)}, GUILayout.Width(20),GUILayout.Height(18)))
            {
                if (_targetMonoScript != monoScript)
                {
                    _targetMonoScript = monoScript;
                    FindInstanceAssets(monoScript.GetClass());
                }
                else
                {
                    _targetMonoScript = null;
                }
            }

            GUI.backgroundColor = globalColor;
        }

        private void DrawInstanceAssetsOfTarget(MonoScript monoScript)
        {
            if (_targetMonoScript == null || _targetMonoScript != monoScript ||
                _instanceAssetsOfTargetMonoScript.Length <= 0) return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var type = monoScript.GetClass();
            var globalEnabled = GUI.enabled;
            GUI.enabled = false;
            foreach (var asset in _instanceAssetsOfTargetMonoScript)
            {
                EditorGUILayout.ObjectField(GUIContent.none, asset, type, false);
            }

            GUI.enabled = globalEnabled;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        private void FindInstanceAssets(Type type)
        {
            var assetGUIDs = AssetDatabase.FindAssets($"t:{type.Name}");
            _instanceAssetsOfTargetMonoScript = assetGUIDs.Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => AssetDatabase.LoadAssetAtPath(p, type)).ToArray();
        }

        private static void CreateAsset(MonoScript monoScript)
        {
            var selected = Selection.activeObject;
            var assetPath = AssetDatabase.GetAssetPath(selected);
            var type = monoScript.GetClass();
            ChildAssetManagerWindow.CreateAssetOfType(type, CreateAssetName(type), assetPath);
        }

        private static string CreateAssetName(MemberInfo type)
        {
            return type.Name;
        }

        private void UpdateOptionList()
        {
            var regex = CreateSearchRegex(_searchName);
            _searchMatchedScripts = _allMonoScripts.Where(t => regex.IsMatch(t.GetClass().Name)).ToArray();
        }

        private static Regex CreateSearchRegex(string str)
        {
            return new(string.Join(".*", str.Split(" ")), RegexOptions.IgnoreCase);
        }

        private static bool IsMonoScriptOfScriptableObject(MonoScript ms)
        {
            return IsMonoScriptOfType(ms, typeof(ScriptableObject));
        }

        private static bool IsMonoScriptOfType(MonoScript ms, Type t)
        {
            var type = ms.GetClass();
            return type != null && t.IsAssignableFrom(type);
        }
    }
}