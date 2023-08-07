#if UNITY_EDITOR
using Common.UnityExtend.Serialization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EditorMenuTools
{
    [MenuItem("Tools/Snm/Log all missing scripts/In prefabs")]
    public static void LogAllMissingScriptsInPrefabs()
    {
        SerializeUtility.TraverseAllPrefabsOrdered(prefab =>
        {
            foreach (Transform t in prefab.transform)
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0)
                {
                    Debug.Log($"{prefab.name} {AssetDatabase.GetAssetPath(prefab)}", prefab);
                }
            }
            return false;
        });
    }

    [MenuItem("Tools/Snm/Log all missing scripts/In current scene")]
    public static void LogAllMissingScriptsInCurrentScene()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        foreach(var rootGo in activeScene.GetRootGameObjects())
        {
            foreach (Transform t in rootGo.transform)
            {  
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0)
                {
                    Debug.Log($"{rootGo.name}", rootGo);
                }
            }
        }
    }
}
#endif