#if UNITY_EDITOR
using Screw;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static LocaleTextViewer;

public class LocaleTextViewer
{
    private Vector2 _scrollPosition;
    public void DrawLocaleDisplayText(LocaleDisplayData[] _categoryDisplayText)
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);
        EditorGUILayout.BeginVertical();
        GUIStyle style = new(EditorStyles.textArea)
        {
            richText = true,
            wordWrap = false
        };
        style.normal.textColor = Color.white;
        if (_categoryDisplayText != null)
        {
            var e = GUI.enabled;
            GUI.enabled = false;
            foreach (var category in _categoryDisplayText)
            {
                EditorGUILayout.LabelField(category.title, style);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextArea(category.key, style, GUILayout.Width(EditorGUIUtility.currentViewWidth / 4));
                EditorGUILayout.TextArea(category.value, style, GUILayout.Width(EditorGUIUtility.currentViewWidth / 4));
                EditorGUILayout.TextArea(category.key, style, GUILayout.Width(EditorGUIUtility.currentViewWidth / 4));
                EditorGUILayout.TextArea(category.value, style, GUILayout.Width(EditorGUIUtility.currentViewWidth / 4));
                EditorGUILayout.EndHorizontal();
            }
            GUI.enabled = e;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    public static void EqualizeNewLine(ref string key, ref string valueText)
    {
        var keyNewLines = key.Count(c => c == '\n');
        var valueNewLines = valueText.Count(c => c == '\n');
        if (keyNewLines == valueNewLines) return;
        if (keyNewLines > valueNewLines)
        {
            AppendNewLines(ref valueText, keyNewLines - valueNewLines);
        }
        else if (keyNewLines < valueNewLines)
        {
            AppendNewLines(ref key, valueNewLines - keyNewLines);
        }
        void AppendNewLines(ref string target, int n)
        {
            for (var i = 0; i < n; i++)
            {
                target += '\n';
            }
        }
    }

    public class LocaleDisplayData
    {
        public string title;
        public string key;
        public string value;

        private int _index = 0;
        public LocaleDisplayData(string title)
        {
            _index = 0;
            this.title = $"<color=yellow>{title}</color>:" + "\n";
        }
        public void AppendKeyValue(string keyText, string valueText)
        {
            var color = _index++ % 2 == 0 ? ColorUtility.ToHtmlStringRGB(Color.white) : ColorUtility.ToHtmlStringRGB(new Color(201f / 255f, 201f / 255f, 201f / 255f));

            var key = "" + keyText;
            var value = "" + valueText;

            EqualizeNewLine(ref key, ref value);

            value = value.Replace("<", "^");

            this.key += $"<color=#{color}>{key}</color>\n";
            this.value += $"<color=#{color}>{value}</color>\n";

        }
    }

    [Serializable]
    public class LocaleData
    {
        public Category[] categories;
        public class Category
        {
            public string name;
            public Translation[] Translations;
        }
        public class Translation
        {
            public string key;
            public string ValueText;
        }
    }
}
public class LocalizeAssetViewer
{
    private LocaleResource[] _localeAssets;
    private LocaleResource _localeAsset;
    private LocaleDisplayData[] _categoryDisplayText;
    private readonly LocaleTextViewer _viewer = new();
    public LocalizeAssetViewer()
    {
        FindLocaleAssets();
    }
    public void Draw()
    {
        EditorGUILayout.BeginVertical();
        DrawLocalizeAssetImport();
        _viewer.DrawLocaleDisplayText(_categoryDisplayText);
        EditorGUILayout.EndVertical();
    }
    private void DrawLocalizeAssetImport()
    {
        if (EditorGUILayout.DropdownButton(new GUIContent(_localeAsset?.name), FocusType.Passive))
        {
            var menu = new GenericMenu();
            foreach (var language in _localeAssets)
            {
                menu.AddItem(new GUIContent(language.name), language == _localeAsset, () =>
                {
                    _localeAsset = language;
                    _categoryDisplayText = LocaleAssetToString(_localeAsset);
                });
            }
            menu.ShowAsContext();
        }
    }
    private void FindLocaleAssets()
    {
        if (_localeAssets == null)
        {
            _localeAssets = AssetDatabase.FindAssets($"t: {nameof(LocaleResource)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>)
                .OfType<LocaleResource>().ToArray();
        }
    }
    private LocaleDisplayData[] LocaleAssetToString(LocaleResource asset)
    {
        var categories = asset.Categories.OrderBy(c => c.name).ToArray();
        var str = new LocaleDisplayData[categories.Length];
        for (int i = 0; i < categories.Length; i++)
        {
            var category = categories[i];
            str[i] = new LocaleDisplayData(category.name);
            foreach (var tr in category.Translations)
            {
                str[i].AppendKeyValue(tr.key, tr.ValueText);
            }
        }
        return str;
    }
}
#endif