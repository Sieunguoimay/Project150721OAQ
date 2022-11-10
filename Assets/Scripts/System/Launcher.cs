using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace System
{
    public class Launcher : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        public void LoadGameScene()
        {
            StartCoroutine(LoadScene(sceneName, OnLoadSceneDone));
        }

        private void OnDestroy()
        {
            // UnloadGameScene();
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