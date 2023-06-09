#if UNITY_EDITOR
using Screw;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static LocaleTextViewer;

//[EditorWindowTitle(title = "LocalizationValidateWindow")]
//public class LocalizationValidateWindow : EditorWindow
//{
//    [MenuItem("Tools/Localization/LocalizationValidateWindow")]
//    public static void Open()
//    {
//        GetWindow<LocalizationValidateWindow>(false, "LocalizationValidateWindow", true).Show();
//    }
//    private LocalizeAssetViewer _localizeViewer = new();
//    private void OnGUI()
//    {
//        if (_localizeViewer == null)
//        {
//            _localizeViewer = new LocalizeAssetViewer();
//        }
//        _localizeViewer.Draw();
//    }
//}
[EditorWindowTitle(title = "SheetLocalizationViewer")]
public class SheetLocalizationViewerWindow : EditorWindow
{
    [MenuItem("Tools/Localization/SheetLocalizationViewer")]
    public static void Open()
    {
        GetWindow<SheetLocalizationViewerWindow>(false, "SheetLocalizationViewerWindow", true).Show();
    }
    [SerializeField] private SheetLocalizationCSVViewer _localizeViewer = new();
    private void OnGUI()
    {
        _localizeViewer.Draw();
    }
}
[Serializable]
public class SheetLocalizationCSVViewer
{
    [SerializeField] private string _filePath;
    [SerializeField] private string[] _languages;
    [SerializeField] private string _selectedLanguage;
    [SerializeField] private LocaleData _localeData;

    private readonly LocaleTextViewer _viewer = new();
    private LocaleDisplayData[] _categoryDisplayText;

    public void Draw()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        DrawLocalizeSheetImport();
        DrawLanguageSeletion();
        DrawCommonMenuButton();
        EditorGUILayout.EndHorizontal();
        _viewer.DrawLocaleDisplayText(_categoryDisplayText);
        EditorGUILayout.EndVertical();
    }
    private void DrawCommonMenuButton()
    {
        var moreButton = GUILayout.Button("...", GUILayout.Width(20));
        if (moreButton)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Open Script"), false, () =>
            {
                OpenScriptByType(typeof(SheetLocalizationViewerWindow));
            });
            menu.ShowAsContext();
        }
    }
    private static void OpenScriptByType(Type type)
    {
        foreach (var script in AssetDatabase.FindAssets("t:Script").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<MonoScript>))
        {
            if (script != null && script.GetClass() != null && script.GetClass() == type)
            {
                AssetDatabase.OpenAsset(script);
                Debug.Log(script.GetClass().Name);
                return;
            }
        }
    }

    private void DrawLocalizeSheetImport()
    {
        if (GUILayout.Button(string.IsNullOrEmpty(_filePath) ? "Select CSV File" : _filePath))
        {
            var newPath = EditorUtility.OpenFilePanel("Select File", "", "");
            if (!string.IsNullOrEmpty(newPath) && _filePath != newPath)
            {
                _filePath = newPath;
                ReadFile(_filePath);
            }
        }
    }
    private void DrawLanguageSeletion()
    {
        if (_languages == null)
        {
            return;
        }
        if (EditorGUILayout.DropdownButton(new GUIContent(_selectedLanguage), FocusType.Passive, GUILayout.Width(150)))
        {
            var menu = new GenericMenu();
            foreach (var language in _languages)
            {
                menu.AddItem(new GUIContent(language), language == _selectedLanguage, () =>
                {
                    LoadLanguage(language);
                });
            }
            menu.ShowAsContext();
        }
    }

    private void ReadFile(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            _languages = GetLanguages(lines[0]);
            LoadLanguage(_languages.FirstOrDefault());
        }
    }

    private string[] GetLanguages(string line)
    {
        var languages = new List<string>();
        var cells = line.Split(',');
        for (int i = 1; i < cells.Length; i++)
        {
            var cell = cells[i];
            if (!string.IsNullOrEmpty(cell))
            {
                languages.Add(cell);
            }
        }
        return languages.ToArray();
    }

    private void LoadLanguage(string language)
    {
        if (!string.IsNullOrEmpty(language))
        {
            _selectedLanguage = language;
            _localeData = ParseCSVToLocaleData(_filePath, language);
            _categoryDisplayText = ParseLocaleDataToDisplayText(_localeData);
            //_categoryDisplayText = ParseDataToDisplayText(_filePath, language);
        }
    }

    private LocaleDisplayData[] ParseDataToDisplayText(string filePath, string language)
    {
        var output = new List<LocaleDisplayData>();
        if (!string.IsNullOrEmpty(filePath))
        {
            string[] lines = SplitLines(File.ReadAllText(filePath));
            var currentLanguageColumnIndex = Array.IndexOf(SplitCsvRow(lines[0]), language);
            LocaleDisplayData category = null;
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var cells = SplitCsvRow(line);
                if (cells.Length >= _languages.Length + 1)
                {
                    if (!string.IsNullOrEmpty(cells[0]) && string.IsNullOrEmpty(cells[1]))
                    {
                        if (category != null)
                        {
                            output.Add(category);
                        }
                        category = new LocaleDisplayData(cells[0]);
                    }
                    if (!string.IsNullOrEmpty(cells[0]) && !string.IsNullOrEmpty(cells[1]))
                    {
                        category.AppendKeyValue(Beautify(cells[0]), Beautify(cells[currentLanguageColumnIndex]));
                    }
                }
            }
        }
        return output.OrderBy(c => c.title).ToArray();
    }
    private static LocaleDisplayData[] ParseLocaleDataToDisplayText(LocaleData localeData)
    {
        if (localeData == null) return null;
        var output = new LocaleDisplayData[localeData.categories.Length];
        for (int i = 0; i < localeData.categories.Length; i++)
        {
            LocaleData.Category category = localeData.categories[i];
            var localeDisplayData = new LocaleDisplayData(category.name);
            foreach (var tr in category.Translations)
            {
                localeDisplayData.AppendKeyValue(tr.key, tr.ValueText);
            }
            output[i] = localeDisplayData;
        }
        return output;
    }
    private LocaleData ParseCSVToLocaleData(string filePath, string language)
    {
        var output = new List<LocaleData.Category>();
        if (!string.IsNullOrEmpty(filePath))
        {
            string[] lines = SplitLines(File.ReadAllText(filePath));
            var currentLanguageColumnIndex = Array.IndexOf(SplitCsvRow(lines[0]), language);
            LocaleData.Category category = null;
            var translations = new List<LocaleData.Translation>();
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var cells = SplitCsvRow(line);
                if (cells.Length >= _languages.Length + 1)
                {
                    if (!string.IsNullOrEmpty(cells[0]) && string.IsNullOrEmpty(cells[1]))
                    {
                        if (category != null)
                        {
                            category.Translations = translations.ToArray();
                            output.Add(category);
                        }
                        category = new LocaleData.Category { name = cells[0] };
                    }
                    if (!string.IsNullOrEmpty(cells[0]) && !string.IsNullOrEmpty(cells[1]))
                    {
                        translations.Add(new LocaleData.Translation
                        {
                            key = Beautify(cells[0]),
                            ValueText = Beautify(cells[currentLanguageColumnIndex])
                        });
                    }
                }
            }
        }
        return new LocaleData { categories = output.OrderBy(c => c.name).ToArray() };
    }

    static public string[] SplitLines(string str)
    {
        var regex = new Regex(@"\n(?=(?:[^\""]*\""[^\""]*\"")*(?![^\""]*\""))");
        return regex.Split(str);
    }
    static public string[] SplitCsvRow(string rowText)
    {
        var regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
        return regex.Split(rowText);
    }
    static public string Beautify(string str)
    {
        var regex = new Regex("^\".*\"$");
        return regex.Replace(str, matched =>
        {
            return matched.Value[1..^1];
        }).Replace("\"\"", "\"");
    }
}
#endif