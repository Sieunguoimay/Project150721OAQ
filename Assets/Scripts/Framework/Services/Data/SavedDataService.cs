using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Framework.Services.Data
{
    public interface ISavedDataService
    {
        void Load(string savedId, object obj);
        void MarkDirty(string savedId, object obj);
        void WriteToStorage();
    }

    public class SavedDataService : ISavedDataService
    {
        private readonly string _path;
        private readonly Dictionary<string, object> _tobeSavedObjects = new();

        public SavedDataService(string path)
        {
            _path = path;
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
        }

        public void Load(string savedId, object obj)
        {
            JsonUtility.FromJsonOverwrite(LoadJsonFromLocal(savedId), obj);
        }

        public void MarkDirty(string savedId, object obj)
        {
            if (!_tobeSavedObjects.ContainsKey(savedId))
            {
                _tobeSavedObjects.Add(savedId, obj);
            }
        }

        public void WriteToStorage()
        {
            foreach (var (key, value) in _tobeSavedObjects)
            {
                SaveJsonToLocal(key, JsonUtility.ToJson(value));
            }
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
            return Path.Combine(_path, $"{savedId}.json");
        }
    }
}