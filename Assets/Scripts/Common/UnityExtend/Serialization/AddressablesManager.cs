using Common;
using Common.UnityExtend.Reflection;
using Common.UnityExtend.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressablesManager : ScriptableObject
{
    [SerializeField] private PathGroup[] groups;

    [Serializable]
    public class PathGroup
    {
        public string name;
        public string[] assetPaths;
    }

    public Dictionary<string, AsyncOperationHandle<UnityEngine.Object>> _operationDictionary;
    private AsyncOperationHandle<IList<UnityEngine.Object>> _handler;
    private PathGroup _currentPathGroup;
    private Action _onDone;

    public IReadOnlyList<PathGroup> Groups => groups;

    public static AddressablesManager Instance { get; private set; }

    public void CreateInstance()
    {
        Instance = this;
    }
    public UnityEngine.Object GetRuntimeAssetByAddress(string path)
    {
        if (_operationDictionary.TryGetValue(path, out var obj))
        {
            return obj.Result;
        }
        Debug.LogError($"Cannot found {path}. Current group is {_currentPathGroup.name}");
        return null;
    }
    public void LoadGroup(string groupName, Action onDone)
    {
        var group = Groups.FirstOrDefault(g => g.name == groupName);
        if (group == null)
        {
            return;
        }
        var assetPaths = group.assetPaths;
        _onDone = onDone;
        _currentPathGroup = group;
        PublicExecutor.Instance.StartCoroutine(LoadAndAssociateResultWithKey(assetPaths, () =>
        {
            onDone?.Invoke();
        }));
    }

    public void ReleaseCurrentGroup()
    {
        foreach (var item in _operationDictionary)
        {
            Addressables.Release(item.Value);
        }
    }

    IEnumerator LoadAndAssociateResultWithKey(IList<string> keys, Action onDone)
    {
        _operationDictionary ??= new Dictionary<string, AsyncOperationHandle<UnityEngine.Object>>();

        var locations = Addressables.LoadResourceLocationsAsync(keys,
                Addressables.MergeMode.Union, typeof(UnityEngine.Object));

        yield return locations;

        var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

        foreach (IResourceLocation location in locations.Result)
        {
            var handle = Addressables.LoadAssetAsync<UnityEngine.Object>(location);
            handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
            loadOps.Add(handle);
        }

        yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

        onDone?.Invoke();
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(ForceUpdateAssetSelector))]
    public void ForceUpdateAssetSelector() => ForceUpdateAssetSelectors();

    [ContextMenu(nameof(CacheAddressableAddressAndUpdateAssetSelector))]
    public void CacheAddressableAddressAndUpdateAssetSelector()
    {
        CachePathsFromAddressables();
        ForceUpdateAssetSelector();
    }

    [MenuItem("Tools/Addressables/ForceUpdateAssetSelectors")]
    public static void ForceUpdateAssetSelectors()
    {
        var type = typeof(AssetSelector);
        SerializeUtility.TraverseAllUnityScriptableAssets((o) =>
        {
            EditorUtility.DisplayProgressBar("ForceUpdateAssetSelectors", $"{o.name}", 0f);
            var modified = false;
            foreach (var f in o.GetType().GetFields(ReflectionUtility.FieldFlags).Where(f => f.FieldType.Equals(type)))
            {
                Debug.Log($"{f.Name} {o.name} {AssetDatabase.GetAssetPath(o)}");
                (f.GetValue(o) as AssetSelector)?.UpdateSerialize();
                modified = true;
            }
            return modified;
        });
        EditorUtility.ClearProgressBar();
    }

    [ContextMenu(nameof(ForceRelease))]
    public void ForceRelease()
    {
        ReleaseCurrentGroup();
    }

    [ContextMenu(nameof(CachePathsFromAddressables))]
    public void CachePathsFromAddressables()
    {
        var setting = AddressableAssetSettingsDefaultObject.Settings;
        var addressableGroups = setting.groups;
        groups = new PathGroup[addressableGroups.Count];
        for (int i = 0; i < addressableGroups.Count; i++)
        {
            groups[i] = new PathGroup
            {
                name = addressableGroups[i].name,
                assetPaths = TraverseAddressableEntries(addressableGroups[i]).Select(e => e.address).ToArray()
            };
        }
    }

    public static AddressableAssetEntry GetAddressableEntryForAsset(UnityEngine.Object asset)
    {
        return TraverseAddressableEntries().FirstOrDefault(e => e.TargetAsset == asset);
    }
    public static AddressableAssetEntry GetAddressableEntryForAddress(string address)
    {
        return TraverseAddressableEntries().FirstOrDefault(e => e.address.Equals(address));
    }

    public static IEnumerable<AddressableAssetEntry> TraverseAddressableEntries()
    {
        var setting = AddressableAssetSettingsDefaultObject.Settings;
        foreach (var g in setting.groups)
        {
            foreach (var e in TraverseAddressableEntries(g))
            {
                yield return e;
            }
        }
    }

    public static IEnumerable<AddressableAssetEntry> TraverseAddressableEntries(AddressableAssetGroup group)
    {
        foreach (var e in group.entries)
        {
            if (e == null) continue;

            yield return e;

            foreach (var s in TraverseAddressableSubEntries(e))
            {
                yield return s;
            }
        }
    }

    private static IEnumerable<AddressableAssetEntry> TraverseAddressableSubEntries(AddressableAssetEntry entry)
    {
        foreach (var e in GetSubAssets(entry))
        {
            yield return e;
        }
        if (entry.SubAssets == null) yield break;
        foreach (var e in entry.SubAssets)
        {
            if (e == null) continue;

            yield return e;
            foreach (var s in TraverseAddressableSubEntries(e))
            {
                yield return s;
            }
        }
    }

    private static IEnumerable<AddressableAssetEntry> GetSubAssets(AddressableAssetEntry entry)
    {
        var list = new List<AddressableAssetEntry>();
        entry.GatherAllAssets(list, false, true, true, null);
        foreach (var e in list)
        {
            if (e == null) continue;
            yield return e;
        }
    }
#endif
}