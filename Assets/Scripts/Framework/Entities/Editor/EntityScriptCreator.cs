using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.Entities.Editor
{
    public class EntityScriptCreator : EditorWindow
    {
        private const string TemplatePath = "Assets/Scripts/Framework/Entities/Editor"; //Copy and paste it here

        [MenuItem("Assets/Create/Entity/Entity Script", priority = 1)]
        public static void Open()
        {
            var window = (EntityScriptCreator) GetWindow(typeof(EntityScriptCreator), true, "Entity Creator");
            window._path = AssetDatabase.GetAssetPath(Selection.activeObject);
            window.maxSize = new Vector2(350, 350);
            window.Show();
        }

        private string _path;
        private string _entityName;
        private bool _createManualView;
        private bool _createInjectableView;

        private string _entityScriptTemplate = "EntityScriptTemplate";
        private string _entityDataScriptTemplate = "EntityDataScriptTemplate";
        private string _entityManualViewScriptTemplate = "EntityManualViewScriptTemplate";
        private string _entityViewScriptTemplate = "EntityViewScriptTemplate";
        private TextAsset _entityScriptTemplateAsset;
        private TextAsset _entityDataScriptTemplateAsset;
        private TextAsset _entityManualViewScriptTemplateAsset;
        private TextAsset _entityViewScriptTemplateAsset;

        private void OnEnable()
        {
            _entityScriptTemplateAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetTemplateProjectPath(_entityScriptTemplate));
            _entityDataScriptTemplateAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetTemplateProjectPath(_entityDataScriptTemplate));
            _entityManualViewScriptTemplateAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetTemplateProjectPath(_entityManualViewScriptTemplate));
            _entityViewScriptTemplateAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetTemplateProjectPath(_entityViewScriptTemplate));
        }

        private void OnGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(this), typeof(EntityScriptCreator),
                false);
            EditorGUILayout.ObjectField(_entityScriptTemplate,_entityScriptTemplateAsset, typeof(TextAsset), false);
            EditorGUILayout.ObjectField(_entityDataScriptTemplate,_entityDataScriptTemplateAsset, typeof(TextAsset), false);
            EditorGUILayout.ObjectField(_entityManualViewScriptTemplate,_entityManualViewScriptTemplateAsset, typeof(TextAsset), false);
            EditorGUILayout.ObjectField(_entityViewScriptTemplate,_entityViewScriptTemplateAsset, typeof(TextAsset), false);
            GUI.enabled = true;

            EditorGUILayout.LabelField($"Path: {_path}");
            GUI.SetNextControlName(nameof(EntityScriptCreator));
            _entityName = EditorGUILayout.TextField("Entity Name", _entityName);
            EditorGUI.FocusTextInControl(nameof(EntityScriptCreator));
            _createInjectableView = EditorGUILayout.Toggle("Create Injectable View", _createInjectableView);
            _createManualView = EditorGUILayout.Toggle("Create Manual View", _createManualView);
            if (GUILayout.Button("Create"))
            {
                if (Create())
                {
                    AssetDatabase.Refresh();
                    Close();
                }
            }
        }

        public bool Create()
        {
            var physicalPath = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? string.Empty, _path,
                _entityName);

            //check if directory doesn't exit
            if (!Directory.Exists(physicalPath))
            {
                //if it doesn't, create it
                Directory.CreateDirectory(physicalPath);
            }

            var nameSpace = CreateNameSpace(Path.Combine(_path, _entityName));

            if (!CreateWithTemplate(GetTemplateAbsolutePath(_entityScriptTemplate), physicalPath, nameSpace,
                _entityName, $"{_entityName}.cs"))
                return false;
            if (!CreateWithTemplate(GetTemplateAbsolutePath(_entityDataScriptTemplate), physicalPath, nameSpace,
                _entityName,
                $"{_entityName}Data.cs")) return false;
            if (!(_createManualView && CreateWithTemplate(GetTemplateAbsolutePath(_entityManualViewScriptTemplate),
                physicalPath,
                nameSpace, _entityName, $"{_entityName}EntityManualView.cs"))) return false;
            return _createInjectableView && CreateWithTemplate(GetTemplateAbsolutePath(_entityViewScriptTemplate),
                physicalPath,
                nameSpace, _entityName, $"{_entityName}EntityView.cs");
        }

        private static string GetTemplateAbsolutePath(string templateName)
        {
            return Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? string.Empty,
                $"{TemplatePath}/{templateName}.txt");
        }

        private string GetTemplateProjectPath(string templateName)
        {
            return $"{TemplatePath}/{templateName}.txt";
        }

        private static bool CreateWithTemplate(string templatePath, string physicalPath, string nameSpace,
            string scriptName, string scriptFileName)
        {
            var entityDataFileContent = CreateScriptFile(templatePath, scriptName, nameSpace);
            var entityDataFilePath = Path.Combine(physicalPath, scriptFileName);

            entityDataFilePath = MakeSureFileName(entityDataFilePath);
            if (!string.IsNullOrEmpty(entityDataFilePath))
            {
                File.WriteAllText(entityDataFilePath, entityDataFileContent);
            }
            else
            {
                return false;
            }

            return true;
        }

        public static string MakeSureFileName(string path)
        {
            var p = path;
            var fileCount = 0;
            while (File.Exists(p))
            {
                p = $"{path}_{fileCount++}";
                if (fileCount > 100)
                {
                    Debug.LogError("Shit!!!");
                    return null;
                }
            }

            return p;
        }

        private static string CreateNameSpace(string assetPath)
        {
            return string.Join('.',
                assetPath.Replace("Assets/Scripts/", "").Replace("Assets\\Scripts\\", "").Split('/', '\\'));
        }

        private static string CreateScriptFile(string templatePath, string entityName, string nameSpace)
        {
            var str = File.ReadAllText(templatePath);
            return str.Replace("#SCRIPTNAME#", entityName).Replace("#NAMESPACE#", nameSpace);
        }
    }
}