using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.UnityExtend.Serialization
{
    public static class CommonMenuItems
    {
        private const string LauncherScene = "Assets/Scenes/Launcher.unity";
        private const string GameScene = "Assets/Scenes/GameScene.unity";
        
        [MenuItem("CONTEXT/ScriptableObject/Ping me!")]
        private static void PingMe(MenuCommand m)
        {
            var o = m.context as ScriptableObject;
            EditorGUIUtility.PingObject(o);
        }

        [MenuItem("Scene/Play Launcher Scene!")]
        private static void PlayLauncherScene(MenuCommand m)
        {
            if (!SceneManager.GetActiveScene().path.Equals(LauncherScene))
            {
                EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene(LauncherScene);
            }
            EditorApplication.isPlaying = true;
        }
        [MenuItem("Scene/Open GameScene")]
        private static void OpenGameScene(MenuCommand m)
        {
            if (!SceneManager.GetActiveScene().path.Equals(GameScene))
            {
                EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene(GameScene);
            }
        }
    }
}