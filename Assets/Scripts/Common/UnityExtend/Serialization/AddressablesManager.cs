using Common;
using Common.UnityExtend.Reflection;
using Common.UnityExtend.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Obsolete]
public class AddressablesManager : ScriptableObject
{
    [SerializeField] private PathGroup[] groups;

    [Serializable]
    public class PathGroup
    {
        public string name;
        public CachedAddress[] assetPaths;
    }
    [Serializable]
    public class CachedAddress
    {
        public string address;
        public string type;
        public AssetReference reference;
    }

    public Dictionary<string, UnityEngine.Object> _dictionary;
    public List<AsyncOperationHandle> _operations;

    private AsyncOperationHandle<IList<UnityEngine.Object>> _handler;
    private PathGroup _currentPathGroup;
    private Action _onDone;

    public IReadOnlyList<PathGroup> Groups => groups;

    public static AddressablesManager Instance { get; private set; }

    public void CreateInstance()
    {
        Instance = this;
    }
    public UnityEngine.Object GetRuntimeAssetByAddress(string path, Type type)
    {
        if (_dictionary == null) return null;
        if (_dictionary.TryGetValue(path + $"|{type.AssemblyQualifiedName}", out var obj))
        {
            return obj;
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

        _operations ??= new List<AsyncOperationHandle>();
        _dictionary ??= new Dictionary<string, UnityEngine.Object>();

        PublicExecutor.Instance.StartCoroutine(LoadAndAssociateResultWithKey(assetPaths, onDone));
    }

    public void ReleaseCurrentGroup()
    {
        foreach (var item in _operations)
        {
            Addressables.Release(item);
        }
        _operations = null;
        _dictionary = null;
    }

    IEnumerator LoadAndAssociateResultWithKey(IList<CachedAddress> keys, Action onDone)
    {
        var reference = keys.FirstOrDefault().reference;
        var addr = keys.FirstOrDefault().address;
        var t = typeof(Sprite);// Type.GetType(keys.FirstOrDefault().type);

        //var handle1 = reference.LoadAssetAsync<UnityEngine.Sprite>();
        //yield return handle1;

        var loadAssetAsyncMeth = typeof(AssetReference).GetMethod("LoadAssetAsync").MakeGenericMethod(t);
        var handle1 = loadAssetAsyncMeth.Invoke(reference, null);

        //Type genericAsyncOpHandleType = typeof(AsyncOperationHandle<>).MakeGenericType(t);
        //var convertMeth = genericAsyncOpHandleType.GetMethod("Convert").MakeGenericMethod(typeof(UnityEngine.Object));

        //var handle2 = convertMeth.Invoke(handle1, null);
        //AsyncOperationHandle ok = (AsyncOperationHandle)(IEnumerator)handle1;

        //if (handle1.GetType().IsInstanceOfType(typeof(AsyncOperationHandle)))
        //{
        yield return (IEnumerator)handle1;
        Type genericAsyncOpHandleType = typeof(AsyncOperationHandle<>).MakeGenericType(t);

        var resultProp = genericAsyncOpHandleType.GetProperty("Result");

        var sprite = resultProp.GetValue(handle1) as UnityEngine.Object;

        Debug.Log("OK");
        //}

        yield break;

        var locationsHandle = Addressables.LoadResourceLocationsAsync(keys.FirstOrDefault().address);
        yield return locationsHandle;

        Debug.Log(string.Join(",", locationsHandle.Result.Select(l => l.PrimaryKey + " " + l.ResourceType))); ;

        //foreach(var l in locationsHandle.Result)
        //{
        //var handle = Addressables.LoadAssetAsync<UnityEngine.Object>(l);
        var handle = Addressables.LoadAssetsAsync<UnityEngine.Object>(locationsHandle.Result, obj =>
        {
            Debug.Log($"Loaded {obj.name} {obj.GetType().Name}");
        });
        yield return handle;
        Debug.Log(string.Join(",", handle.Result.Select(l => l.name + " " + l.GetType().Name))); ;
        //}

        //var loadLocationsHandles = Addressables.LoadResourceLocationsAsync(keys.Select(k => k.address),
        //        Addressables.MergeMode.Union);
        //yield return loadLocationsHandles;

        //var loadAssetHandles = new List<AsyncOperationHandle>(loadLocationsHandles.Result.Count);

        //for (int i = 0; i < loadLocationsHandles.Result.Count; i++)
        //{
        //    var location = loadLocationsHandles.Result[i];
        //    var handle = Addressables.LoadAssetAsync<UnityEngine.Object>("");
        //    handle.Completed += obj => _operationDictionary.Add(location.PrimaryKey, obj);
        //    loadAssetHandles.Add(handle);
        //}
        var loadAssetHandles = new List<AsyncOperationHandle>();
        foreach (var k in keys)
        {
            var address = k.address;
            //if (Type.GetType(k.type) == typeof(Sprite))
            //{
            //    var handle = Addressables.LoadAssetsAsyn<UnityEngine.Object>(address, null);
            //    handle.Completed += obj => OnSpriteAssetLoaded(address, obj);// _operationDictionary.Add(location.PrimaryKey, obj);
            //    loadAssetHandles.Add(handle);
            //}
            //else
            //{
            //var loadAssetAsync = typeof(Addressables).GetMethod("LoadAssetAsync", new[] { typeof(string) }).MakeGenericMethod(Type.GetType(k.type));
            //var handle = loadAssetAsync.Invoke(null, new object[] { address });// Addressables.LoadAssetAsync<UnityEngine.Object>(address);

            //Type genericAsyncOpHandleType = typeof(AsyncOperationHandle<>).MakeGenericType(Type.GetType(k.type));

            //try
            //{
            //    var _handled = (AsyncOperationHandle<UnityEngine.Object>)handle;
            //}catch(Exception e)
            //{
            //    Debug.Log(e.Message);

            //}

            //var handle = Addressables.LoadAssetAsync<UnityEngine.Object>(address);

            //handle.Completed += obj => OnAssetLoaded(address, obj);// _operationDictionary.Add(location.PrimaryKey, obj);
            //loadAssetHandles.Add(handle);
            //}
        }

        yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadAssetHandles, true);

        onDone?.Invoke();
    }
    private void OnAssetLoaded(string address, AsyncOperationHandle handle)
    {
        _operations.Add(handle);
        var asset = handle.Result as UnityEngine.Object;
        _dictionary.Add(address + $"|{asset.GetType().AssemblyQualifiedName}", asset);
    }
    private void OnSpriteAssetLoaded(string address, AsyncOperationHandle<IList<UnityEngine.Object>> handle)
    {
        _operations.Add(handle);
        var asset = handle.Result;
        //_dictionary.Add(address + $"|{asset.GetType().AssemblyQualifiedName}", asset);
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
                assetPaths = TraverseAddressableEntries(addressableGroups[i])
                .Where(e => !e.IsFolder)
                .Select(CreateAddress).ToArray()
            };
        }
    }
    private bool IsMainAsset(AddressableAssetEntry e)
    {
        return !string.IsNullOrEmpty(e.guid);
    }
    private CachedAddress CreateAddress(AddressableAssetEntry e)
    {
        return new CachedAddress
        {
            address = GetSubAssetName(e),
            type = e.MainAssetType.AssemblyQualifiedName
        };
    }
    private string GetSubAssetName(AddressableAssetEntry e)
    {
        if (IsMainAsset(e)) return e.guid;
        var str = e.address;

        string pattern = @"\[([^\]]*)\]$";
        Match match = Regex.Match(str, pattern);
        if (match.Success)
        {
            string substringInsideBrackets = match.Groups[1].Value;
            return $"{e.AssetPath}[{substringInsideBrackets}]";
        }
        return "";
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