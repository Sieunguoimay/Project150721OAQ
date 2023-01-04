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

        private bool _foldout;
        private readonly TemplateSet _templateSet = new();
        private bool _useContainerTemplate;
        private void OnEnable()
        {
            _templateSet.Setup("EntityScriptTemplate", "EntityDataScriptTemplate", "EntityManualViewScriptTemplate", "EntityViewScriptTemplate");
        }

        private void OnGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(this), typeof(EntityScriptCreator), false);
            GUI.enabled = true;
            var useContainerTemplate = EditorGUILayout.Toggle("Use Container Template",_useContainerTemplate);
            GUI.enabled = false;
            if (_useContainerTemplate != useContainerTemplate)
            {
                _useContainerTemplate = useContainerTemplate;
                if (_useContainerTemplate)
                {
                    _templateSet.Setup("Container/ContainerEntityScriptTemplate", "Container/ContainerEntityDataScriptTemplate",
                        "Container/ContainerEntityManualViewScriptTemplate", "Container/ContainerEntityViewScriptTemplate");
                }
                else
                {
                    _templateSet.Setup("EntityScriptTemplate", "EntityDataScriptTemplate", "EntityManualViewScriptTemplate", "EntityViewScriptTemplate");
                }
            }
            _foldout = EditorGUILayout.Foldout(_foldout, "Templates", true);
            if (_foldout)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(10, false);
                EditorGUILayout.BeginVertical();
                _templateSet.DrawTemplates();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

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
            return _templateSet.Create(_entityName, physicalPath, nameSpace, _createManualView, _createInjectableView);
        }

        private class TemplateSet
        {
            private string _entityScriptTemplate = "EntityScriptTemplate";
            private string _entityDataScriptTemplate = "EntityDataScriptTemplate";
            private string _entityManualViewScriptTemplate = "EntityManualViewScriptTemplate";
            private string _entityViewScriptTemplate = "EntityViewScriptTemplate";

            private TextAsset _entityScriptTemplateAsset;
            private TextAsset _entityDataScriptTemplateAsset;
            private TextAsset _entityManualViewScriptTemplateAsset;
            private TextAsset _entityViewScriptTemplateAsset;

            public void Setup(string a, string b, string c, string d)
            {
                _entityScriptTemplate = a;
                _entityDataScriptTemplate = b;
                _entityManualViewScriptTemplate = c;
                _entityViewScriptTemplate = d;
                _entityScriptTemplateAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetTemplateProjectPath(_entityScriptTemplate));
                _entityDataScriptTemplateAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetTemplateProjectPath(_entityDataScriptTemplate));
                _entityManualViewScriptTemplateAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetTemplateProjectPath(_entityManualViewScriptTemplate));
                _entityViewScriptTemplateAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetTemplateProjectPath(_entityViewScriptTemplate));
            }

            public void DrawTemplates()
            {
                EditorGUILayout.ObjectField("Entity", _entityScriptTemplateAsset, typeof(TextAsset), false);
                EditorGUILayout.ObjectField("EntityData", _entityDataScriptTemplateAsset, typeof(TextAsset), false);
                EditorGUILayout.ObjectField("EntityView", _entityViewScriptTemplateAsset, typeof(TextAsset), false);
                EditorGUILayout.ObjectField("ManualEntityView", _entityManualViewScriptTemplateAsset, typeof(TextAsset), false);
            }

            public bool Create(string entityName, string physicalPath, string nameSpace, bool createManualView, bool createInjectableView)
            {
                if (!CreateWithTemplate(GetTemplateAbsolutePath(_entityScriptTemplate), physicalPath, nameSpace,
                    entityName, $"{entityName}.cs"))
                    return false;
                if (!CreateWithTemplate(GetTemplateAbsolutePath(_entityDataScriptTemplate), physicalPath, nameSpace,
                    entityName,
                    $"{entityName}Data.cs")) return false;
                if (!(createManualView && CreateWithTemplate(GetTemplateAbsolutePath(_entityManualViewScriptTemplate),
                    physicalPath,
                    nameSpace, entityName, $"{entityName}EntityManualView.cs"))) return false;
                return createInjectableView && CreateWithTemplate(GetTemplateAbsolutePath(_entityViewScriptTemplate),
                    physicalPath,
                    nameSpace, entityName, $"{entityName}EntityView.cs");
            }
        }

        private static string GetTemplateAbsolutePath(string templateName)
        {
            return Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? string.Empty,
                $"{TemplatePath}/{templateName}.txt");
        }

        private static string GetTemplateProjectPath(string templateName)
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