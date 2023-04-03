using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace System
{
    public class GameEngine : MonoBehaviour
    {
#if UNITY_EDITOR
        [StringSelector(nameof(FindScenes))]
#endif
        [SerializeField] private string sceneName;
        [SerializeField] private EntityController entityController;

        #if UNITY_EDITOR
        public IEnumerable<string> FindScenes() => AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<SceneAsset>).Select(s=> s.name);
        #endif
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            entityController.Load();
            LoadGameScene();
        }

        private void OnDestroy()
        {
            // UnloadGameScene();
            entityController.Unload();
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
    }
}