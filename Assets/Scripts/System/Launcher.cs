using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace System
{
    public class Launcher : MonoBehaviour
    {
        private void Start()
        {
            SpawnGame();
        }

        private void OnDestroy()
        {
            KillGame();
        }

        public void SpawnGame()
        {
            StartCoroutine(LoadScene("Main", OnLoadSceneDone));
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

        public void KillGame()
        {
        }
    }
}