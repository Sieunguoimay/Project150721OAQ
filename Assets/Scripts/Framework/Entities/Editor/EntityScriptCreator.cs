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
            window.maxSize = new Vector2(350, 200);
            window.Show();
        }

        private string _path;
        private string _entityName;
        private bool _useManualView;

        private void OnGUI()
        {
            EditorGUILayout.LabelField($"Path: {_path}");
            GUI.SetNextControlName(nameof(EntityScriptCreator));
            _entityName = EditorGUILayout.TextField("Entity Name", _entityName);
            EditorGUI.FocusTextInControl(nameof(EntityScriptCreator));
            _useManualView = EditorGUILayout.Toggle("Create Manual View", _useManualView);
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
            var result = true;
            if (_useManualView)
            {
                result = CreateWithTemplate("EntityManualViewScriptTemplate", physicalPath, nameSpace, _entityName,
                    $"{_entityName}EntityManualView.cs");
            }

            var result2 = CreateWithTemplate("EntityScriptTemplate", physicalPath, nameSpace, _entityName,
                              $"{_entityName}.cs") &&
                          CreateWithTemplate("EntityDataScriptTemplate", physicalPath, nameSpace, _entityName,
                              $"{_entityName}Data.cs") &&
                          CreateWithTemplate("EntityViewScriptTemplate", physicalPath, nameSpace, _entityName,
                              $"{_entityName}EntityView.cs");
            return result && result2;

            // var entityFileContent = CreateScriptFile("EntityScriptTemplate", _entityName, nameSpace);
            // var entityFilePath = Path.Combine(physicalPath, $"{_entityName}.cs");
            //
            // entityFilePath = MakeSureFileName(entityFilePath);
            // if (!string.IsNullOrEmpty(entityFilePath))
            // {
            //     File.WriteAllText(entityFilePath, entityFileContent);
            // }
            // else
            // {
            //     return false;
            // }
            //
            // var entityDataFileContent = CreateScriptFile("EntityDataScriptTemplate", _entityName, nameSpace);
            // var entityDataFilePath = Path.Combine(physicalPath, $"{_entityName}Data.cs");
            //
            // entityDataFilePath = MakeSureFileName(entityDataFilePath);
            // if (!string.IsNullOrEmpty(entityDataFilePath))
            // {
            //     File.WriteAllText(entityDataFilePath, entityDataFileContent);
            // }
            // else
            // {
            //     return false;
            // }
        }

        private static bool CreateWithTemplate(string templateName, string physicalPath, string nameSpace,
            string scriptName,
            string scriptFileName)
        {
            var entityDataFileContent = CreateScriptFile(templateName, scriptName, nameSpace);
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

        private static string CreateScriptFile(string templateName, string entityName, string nameSpace)
        {
            var str = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? string.Empty,
                $"{TemplatePath}/{templateName}.txt"));
            return str.Replace("#SCRIPTNAME#", entityName).Replace("#NAMESPACE#", nameSpace);
        }
    }
}