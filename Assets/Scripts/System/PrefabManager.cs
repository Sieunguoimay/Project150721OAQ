using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "PrefabManager")]
public partial class PrefabManager : ScriptableObject
{
    [SerializeField] private List<GameObject> prefabs = new List<GameObject>();
    private Dictionary<string, GameObject> maps = new Dictionary<string, GameObject>();

    public void Setup()
    {
        foreach (var prefab in prefabs)
        {
            var mc = prefab.GetComponent<MasterComponent>();
            if (mc)
            {
                if (maps.ContainsKey(mc.UniqueID))
                {
                    Debug.LogError($"Prefab {mc.MasterType.Name} already existed");
                }
                else
                {
                    maps.Add(mc.UniqueID, prefab);
                }
            }
            else
            {
                Debug.LogError($"Given gameobject doesnot contain a master component: {prefab.name}");
            }
        }
    }

    public GameObject GetPrefab(string id)
    {
        return maps[id];
    }

    public GameObject GetPrefab<T>() where T : MasterComponent
    {
        return GetPrefab(typeof(T).Name);
    }

    public void AddPrefab(GameObject prefab)
    {
        var mc = prefab.GetComponent<MasterComponent>();
        if (mc)
        {
            prefabs.Add(prefab);
            maps.Add(mc.UniqueID, prefab);
        }
        else
        {
            Debug.LogError($"Given gameobject doesnot contain a master component: {prefab.name}");
        }
    }
}

#if UNITY_EDITOR

public partial class PrefabManager
{
    [HideInInspector] public string prefabName;
}

[CustomEditor(typeof(PrefabManager))]
public class PrefabManagerEditor : Editor
{
    private PrefabManager prefabManager;
    private PrefabManager PrefabManager => prefabManager ?? (prefabManager = target as PrefabManager);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        PrefabManager.prefabName = EditorGUILayout.TextField("Prefab Name", PrefabManager.prefabName);
        var createScriptClicked = GUILayout.Button("Create Script", GUILayout.Width(90));
        var createPrefabClicked = GUILayout.Button("Create Prefab", GUILayout.Width(90));
        EditorGUILayout.EndHorizontal();

        var prefabName = PrefabManager.prefabName;

        if (createScriptClicked)
        {
            // string currentPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(PrefabManager));
            CreateScript("Assets\\Scripts", prefabName, nameof(MasterComponent));
        }

        if (createPrefabClicked)
        {
            string localPath = "Assets\\Prefabs\\" + prefabName + ".prefab";

            var go = new GameObject(prefabName);
            go.AddComponent(Type.GetType(prefabName));
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go,
                AssetDatabase.GenerateUniqueAssetPath(localPath),
                InteractionMode.UserAction);
            DestroyImmediate(go);
            PrefabManager.prefabName = "";
            PrefabManager.AddPrefab(prefab);
        }
    }

    static void CreateScript(string path, string name, string baseClass)
    {
        // remove whitespace and minus
        name = name.Replace("-", "_");
        string copyPath = path + "/" + name + ".cs";
        Debug.Log("Creating Classfile: " + copyPath);
        if (File.Exists(copyPath) == false)
        {
            // do not overwrite
            using (StreamWriter outfile =
                new StreamWriter(copyPath))
            {
                outfile.WriteLine("using System;");
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using System.Collections;");
                outfile.WriteLine("[DisallowMultipleComponent]");
                outfile.WriteLine($"public class {name} : {baseClass} {{");
                // outfile.WriteLine(" ");
                // outfile.WriteLine(" ");
                // outfile.WriteLine(" public override Type Type=>GetType();");
                // outfile.WriteLine(" // Use this for initialization");
                // outfile.WriteLine(" void Start () {");
                // outfile.WriteLine(" ");
                // outfile.WriteLine(" }");
                // outfile.WriteLine(" ");
                // outfile.WriteLine(" ");
                // outfile.WriteLine(" // Update is called once per frame");
                // outfile.WriteLine(" void Update () {");
                // outfile.WriteLine(" ");
                // outfile.WriteLine(" }");
                outfile.WriteLine("}");
            } //File written
        }

        AssetDatabase.Refresh();
    }
}
#endif