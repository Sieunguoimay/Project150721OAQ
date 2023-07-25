using Framework;
using System;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SaveData
{
    public class SaveDataManager : ScriptableObject
    {
        [SerializeField] private string saveFolder = "SavedData";
#if UNITY_EDITOR
        [ContextMenuItem("Load Assets", nameof(LoadAssetsOfTypeSaveDataSO))]
#endif
        [SerializeField] private SaveDataSO[] saveDataItems;

#if UNITY_EDITOR
        public static string ProjectPath => Path.GetDirectoryName(Application.dataPath);
        public string SavedDataPath => Path.Combine(ProjectPath, saveFolder);
#else
        //Todo:..
#endif

        public void Load()
        {
            foreach (var item in saveDataItems)
            {
                Load(item);
            }
        }

        public void Save()
        {
            foreach (var item in saveDataItems)
            {
                Save(item);
            }
        }
        private void Save(SaveDataSO saveData)
        {
            var json = JsonUtility.ToJson(saveData);
            var fileName = saveData.GetSaveFileName();
            SaveJsonToLocal(fileName, json);
        }
        private void Load(SaveDataSO saveData)
        {
            var fileName = saveData.GetSaveFileName();
            var json = LoadJsonFromLocal(fileName);
            JsonUtility.FromJsonOverwrite(json, saveData);
        }

        private string LoadJsonFromLocal(string savedId)
        {
            var filePath = GetSavedFilePath(savedId);

            if (!File.Exists(filePath)) return null;

            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Err: Failed to load json file at path {filePath} - {e.Message}");
            }

            return null;
        }
        private void SaveJsonToLocal(string savedId, string json)
        {
            var filePath = GetSavedFilePath(savedId);
            File.WriteAllText(filePath, json);
            Debug.Log($"Saved {filePath}");
        }
        private string GetSavedFilePath(string savedId)
        {
            return Path.Combine(SavedDataPath, $"{savedId}.json");
        }
#if UNITY_EDITOR
        [ContextMenu("Load Assets")]
        private void LoadAssetsOfTypeSaveDataSO()
        {
            saveDataItems = AssetDatabase.FindAssets($"t: {nameof(SaveDataSO)}", null)
                .Select(AssetDatabase.GUIDToAssetPath)
                .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                .OfType<SaveDataSO>().ToArray();
        }

        [ContextMenu("Clear All")]
        public void ClearSavedData()
        {
            var files = Directory.GetFiles(Path.Combine(EntityController.ProjectPath, SavedDataPath));
            foreach (var file in files)
            {
                File.Delete(file);
                Debug.Log($"{file} is deleted.");
            }
            foreach (var item in saveDataItems)
            {
                item.ResetAll();
            }
        }

#endif
    }
}