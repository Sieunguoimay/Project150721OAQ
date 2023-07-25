using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Framework;
using SaveData;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
//using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace System
{
    public class GameSceneLoader : MonoBehaviour
    {
#if UNITY_EDITOR
        [StringSelector(nameof(FindScenes))]
#endif
        [SerializeField] private string sceneName;
        [SerializeField] private EntityController entityController;
        [SerializeField] private SaveDataManager saveDataManager;
        //[SerializeField] private AddressablesManager addressablesManager;
        //[SerializeField, StringSelector(nameof(GetAllAssetGroups))]
        //private string assetGroupName;

#if UNITY_EDITOR
        public IEnumerable<string> FindScenes() => AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<SceneAsset>).Select(s => s.name);
        //public IEnumerable<string> GetAllAssetGroups() => addressablesManager?.Groups.Select(g => g.name);
#endif
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            entityController.Load();
            saveDataManager.Load();
            //LoadAddressables(() =>
            //{
                LoadGameScene();
            //});
        }

        private void OnDestroy()
        {
            // UnloadGameScene();
            //UnloadAddressables();
            entityController.Unload();
            saveDataManager.Save();
        }

        private void OnApplicationQuit()
        {
            entityController.UnloadServices();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
            }
            else
            {
                entityController.UnloadServices();
            }
        }

        public void LoadGameScene()
        {
            StartCoroutine(LoadScene(sceneName, OnLoadSceneDone));
        }

        public void UnloadGameScene()
        {
            StartCoroutine(UnloadScene(sceneName, OnLoadSceneDone));
        }

        private void OnLoadSceneDone()
        {
        }

        private static IEnumerator LoadScene(string sceneName, Action onDone)
        {
            var ao = SceneManager.LoadSceneAsync(sceneName);
            while (!ao.isDone)
            {
                yield return null;
            }

            onDone?.Invoke();
        }

        private static IEnumerator UnloadScene(string sceneName, Action onDone)
        {
            var ao = SceneManager.UnloadSceneAsync(sceneName);
            while (!ao.isDone)
            {
                yield return null;
            }

            onDone?.Invoke();
        }
        //private void LoadAddressables(Action onDone)
        //{
            //addressablesManager.CreateInstance();
            //if (string.IsNullOrEmpty(assetGroupName)) return;
            //addressablesManager.LoadGroup(assetGroupName, onDone);
        //}
        //private void UnloadAddressables()
        //{
            //addressablesManager.ReleaseCurrentGroup();
        //}
    }
}