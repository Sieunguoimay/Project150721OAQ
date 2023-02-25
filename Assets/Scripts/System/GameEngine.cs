using System.Collections;
using Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace System
{
    public class GameEngine : MonoBehaviour
    {
        [SerializeField] private string sceneName;
        [SerializeField] private EntityController entityController;

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
            entityController.SaveEntities();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
            }
            else
            {
                entityController.SaveEntities();
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