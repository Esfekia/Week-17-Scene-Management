//  Copyright (c) 2020-present amlovey
//  
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEditor;

namespace APlus2
{
    [Serializable]
    public class AssetCacheItem : ScriptableObject
    {
        [SerializeField]
        public string assetType;
        [NonSerialized]
        public List<APAsset> assets;
        [SerializeField]
        private List<string> raw;

        public static AssetCacheItem CreateInstanceX(string assetType)
        {
            var instance = ScriptableObject.CreateInstance<AssetCacheItem>();
            instance.assetType = assetType;
            return instance;
        }

        public void RawToAssets()
        {
            var type = ModuleHelper.GetAssetTypeClass(assetType);
            this.assets = this.raw.Select(json => JsonUtility.FromJson(json, type) as APAsset).ToList();
        }

        public void AssetsToRaw()
        {
            this.raw = this.assets.Select(JsonUtility.ToJson).ToList();
        }

        private void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }
    }

    [Serializable]
    public class APCache
    {
        [NonSerialized]
        public List<AssetCacheItem> cacheItems;
        [SerializeField]
        private List<string> rawItems;

        /// <summary>
        /// Map to store data to speed up assets search
        /// </summary>
        public Dictionary<string, APAsset> map;
        public Dictionary<string, AssetCacheItem> linkMap;

        public static Func<HashSet<string>> GetAddressablesDelegate;

        public APCache()
        {
            cacheItems = new List<AssetCacheItem>();
            map = new Dictionary<string, APAsset>();
            linkMap = new Dictionary<string, AssetCacheItem>();
        }

        public int GetRawCount()
        {
            return rawItems == null ? 0 : rawItems.Count();
        }

        public void DataToRaw()
        {
            rawItems = this.cacheItems.Select(CacheItemToJson).ToList();
        }

        private string CacheItemToJson(AssetCacheItem item)
        {
            item.AssetsToRaw();
            return JsonUtility.ToJson(item);
        }

        public void RawToData()
        {
            cacheItems = this.rawItems.Select(ToCachItem).ToList();
        }

        private AssetCacheItem ToCachItem(string json)
        {
            var instance = ScriptableObject.CreateInstance<AssetCacheItem>();
            JsonUtility.FromJsonOverwrite(json, instance);
            instance.RawToAssets();
            return instance;
        }

        public void ReBuildMap()
        {
            this.map.Clear();
            this.linkMap.Clear();
            this.cacheItems.ForEach(item => this.AddListToMap(item.assetType, item));
        }

        public List<APAsset> GetAssetsListByType(string type)
        {
            if (linkMap.ContainsKey(type))
            {
                return linkMap[type].assets;
            }

            return null;
        }

        public void AddListToMap(string assetType, AssetCacheItem item)
        {
            if (item != null)
            {
                item.assets.ForEach(AddToMapIfNeeds);
                if (linkMap.ContainsKey(assetType))
                {
                    linkMap[assetType] = item;
                }
                else
                {
                    linkMap.Add(assetType, item);
                }
            }
        }

        public void AddToMapIfNeeds(APAsset asset)
        {
            if (map.ContainsKey(asset.Id))
            {
                map[asset.Id] = asset;
            }
            else
            {
                map.Add(asset.Id, asset);
            }
        }

        public void RemoveAsset(string id, string assetType)
        {
            if (map.ContainsKey(id))
            {
                if (linkMap.ContainsKey(assetType))
                {
                    var asset = linkMap[assetType].assets.Find(a => a.Id == id);
                    linkMap[assetType].assets.Remove(asset);
                }
            }
        }

        public void RemoveAsset(string id)
        {
            if (map.ContainsKey(id))
            {
                var asset = map[id];
                var assetType = ModuleHelper.GetAssetType(asset);
                if (linkMap.ContainsKey(assetType))
                {
                    linkMap[assetType].assets.Remove(asset);
                }
                map.Remove(id);
            }
        }

        public void UpdateAsset(APAsset asset)
        {
            if (map.ContainsKey(asset.Id))
            {
                map[asset.Id] = asset;
                var assetType = ModuleHelper.GetAssetType(asset);
                if (linkMap.ContainsKey(assetType))
                {
                    var item = linkMap[assetType];
                    var index = item.assets.FindIndex(0, _asset => asset.Id == _asset.Id);
                    
                    if (index != -1)
                    {
                        item.assets[index] = asset;
                    }
                }
            }
            else
            {
                AddAsset(asset);
            }
        }

        public void MoveTo(string id, string newPath)
        {
            if (map.ContainsKey(id))
            {
                map[id].Path = newPath;
            }
        }

        public void AddAsset(APAsset asset)
        {
            if (asset == null)
            {
                return;
            }

            var assetType = ModuleHelper.GetAssetType(asset);
            if (linkMap.ContainsKey(assetType))
            {
                linkMap[assetType].assets.Add(asset);
            }

            AddToMapIfNeeds(asset);
        }

        public bool HasAsset(string id)
        {
            return map.ContainsKey(id);
        }

        public APAsset GetAsset(string id)
        {
            if (HasAsset(id))
            {
                return map[id];
            }

            return null;
        }

        public void UpdateUsedStatus(HashSet<string> usedFiles)
        {
            SetAllAssetsToUnUsed();

            // check the assets in :
            //  1. the build report
            //  2. the enabled scenes
            //  3. the AssetBundles
            //  4. the PlayerSettings
            //
            UpdateUnusedStatusInternal(usedFiles);
            UpdateUsedStatusFromScene();
            UpdateUsedStatusFromAssetBundle();
            UpdateUnusedForProjectSettings();
#if ADDRESSABLE_ON
            UpdateUnsedForAddressables();
#endif
        }

#if ADDRESSABLE_ON
        private void UpdateUnsedForAddressables()
        {
            if (GetAddressablesDelegate == null)
            {
                return;
            }

            var addressables = GetAddressablesDelegate(); 
            
            foreach (var item in addressables)
            {
                var asset = GetAsset(AssetDatabase.AssetPathToGUID(item));   
                asset.InAddressables = true;
                UpdateAsset(asset);
            }

            UpdateUnusedStatusInternal(addressables, false);
        }
#endif

        private void UpdateUnusedForProjectSettings()
        {
            HashSet<string> unusedAssetsSet = new HashSet<string>();
            var textures = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
            foreach (var tex in textures)
            {
                var path = AssetDatabase.GetAssetPath(tex);
                if (!string.IsNullOrEmpty(path))
                {
                    unusedAssetsSet.Add(path);
                }
            }

            if (PlayerSettings.defaultCursor != null)
            {
                unusedAssetsSet.Add(AssetDatabase.GetAssetPath(PlayerSettings.defaultCursor));
            }

            UpdateUnusedStatusInternal(unusedAssetsSet);
        }

        private void UpdateUsedStatusFromAssetBundle()
        {
            var assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            HashSet<string> pathSet = new HashSet<string>();
            foreach (var name in assetBundleNames)
            {
                var paths = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPathsFromAssetBundle(name));
                foreach (var path in paths)
                {
                    pathSet.Add(path);
                }
            }

            UpdateUnusedStatusInternal(pathSet, true);
        }

        private void UpdateUsedStatusFromScene()
        {
            var ScenesList = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var unusedAssets = AssetDatabase.GetDependencies(ScenesList);
            HashSet<string> unusedAssetsSet = new HashSet<string>();

            foreach (var path in unusedAssets)
            {
                unusedAssetsSet.Add(path);
            }

            UpdateUnusedStatusInternal(unusedAssetsSet);
        }

        private void UpdateUnusedStatusInternal(HashSet<string> usedFiles, bool includedInAssetBundle = false)
        {
            Utilities.DebugLog("UpdateUnusedStatusInternal");
            foreach (var asset in map.Values)
            {
                if ((Utilities.IsInResources(asset.Path) ||
                    Utilities.IsStreamingAssetsFile(asset.Path)) && !includedInAssetBundle)
                {
                    asset.Used = NullableBoolean.True;
                }
                else
                {
                    string filePath = asset.Path.Replace('\\', '/');

                    // if it's the asset bundle assets set, we just set InAssetBundle to True
                    //
                    if (includedInAssetBundle)
                    {
                        asset.InAssetBundle = usedFiles.Contains(filePath);
                    }
                    else
                    {
                        if (usedFiles.Contains(filePath))
                        {
                            asset.Used = NullableBoolean.True;
                        }
                    }
                }
            }
        }

        private void SetAllAssetsToUnUsed()
        {
            foreach (var asset in map.Values)
            {
                asset.Used = NullableBoolean.False;
            }
        }

        public List<string> GetReferences(string selectedAssetPath, string progressInfo, float startProgress, float endProgress, ref bool cancel)
        {
            string title = "Find references";
            EditorUtility.DisplayCancelableProgressBar(title, progressInfo, startProgress);
            var span = endProgress - startProgress;
            List<string> references = new List<string>();

            references.AddRange(GetReferences(selectedAssetPath, AssetType.PREFABS));
            if (EditorUtility.DisplayCancelableProgressBar(title, progressInfo, startProgress + 0.2f * span))
            {
                cancel = true;
                return references;
            }

            references.AddRange(GetReferences(selectedAssetPath, AssetType.MODELS));
            if (EditorUtility.DisplayCancelableProgressBar(title, progressInfo, startProgress + 0.4f * span))
            {
                cancel = true;
                return references;
            }

            references.AddRange(GetReferences(selectedAssetPath, AssetType.MATERIALS));
            if (EditorUtility.DisplayCancelableProgressBar(title, progressInfo, startProgress + 0.6f * span))
            {
                cancel = true;
                return references;
            }

            references.AddRange(GetReferences(selectedAssetPath, AssetType.OTHERS));
            if (EditorUtility.DisplayCancelableProgressBar(title, progressInfo, startProgress + 0.8f * span))
            {
                cancel = true;
                return references;
            }

            references.AddRange(GetReferencesInScene(selectedAssetPath, ref cancel));
            if (EditorUtility.DisplayCancelableProgressBar(title, progressInfo, startProgress + 1f * span))
            {
                cancel = true;
            }

            return references;
        }

        public List<string> GetReferencesInScene(string selectedAssetPath,
                                                        ref bool cancel,
                                                        bool withProgress = false,
                                                        string progressInfo = "",
                                                        float startProgress = 0,
                                                        float endProcess = 1)
        {
            string title = "Find references";
            var sceneGuids = AssetDatabase.FindAssets("t:scene");
            List<string> references = new List<string>();

            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                var dependences = AssetDatabase.GetDependencies(new string[] { scenePath });
                if (dependences.Any(denpend => denpend.Equals(selectedAssetPath, StringComparison.CurrentCultureIgnoreCase)))
                {
                    references.Add(scenePath);
                }

                if (withProgress)
                {
                    float progress = startProgress + (endProcess - startProgress) * (i + 1) * 1f / sceneGuids.Length;
                    if (EditorUtility.DisplayCancelableProgressBar(title, progressInfo, progress))
                    {
                        cancel = true;
                        break;
                    }
                }
            }

            return references;
        }

        private List<string> GetReferences(string assetPath, string assetType)
        {
            List<string> references = new List<string>();
            var lookForSet = GetAssetsListByType(assetType);
            if (lookForSet == null)
            {
                return references;
            }

            foreach (var asset in lookForSet)
            {
                var dependences = AssetDatabase.GetDependencies(new string[] { asset.Path });
                if (dependences.Any(denpend => denpend.Equals(assetPath, StringComparison.CurrentCultureIgnoreCase)))
                {
                    references.Add(asset.Path);
                }
            }

            return references;
        }

        public void Clear()
        {
            if (this.linkMap != null)
            {
                linkMap.Clear();
            }

            if (this.map != null)
            {
                this.map.Clear();
            }

            if (cacheItems != null)
            {
                cacheItems.Clear();
            }
        }
    }

    public static class CacheManager
    {
        static string ASSETS_PATH = Path.Combine(Application.persistentDataPath, "A+AssetsExplorer.cache2");
        public static APCache cache;
        private static bool isBuldingCache = false;

        [InitializeOnLoadMethod]
        public static void OnLoad()
        {
            if (!IsCacheFileExists())
            {
                LoadDataInToCache(null);
            }
            else
            {
#if APLUS_DEV
                var now = DateTime.Now;
#endif
                var tempCache = new APCache();
                LoadFromLocal<APCache>(ASSETS_PATH, ref tempCache);
#if APLUS_DEV
                Debug.Log(String.Format("OnLoad LoadFromLocal cost {0} ms", (DateTime.Now - now).TotalMilliseconds));
#endif
                if (tempCache.GetRawCount() != ModuleHelper.Categories.Count())
                {
                    var title = "New Category Detected";
                    var message = "New category detected, A+ Assets Explorer will rebuilt cache.";
                    if (EditorUtility.DisplayDialog(title, message, "OK"))
                    {
                        LoadDataInToCache(null);
                    }
                }
            }
        }

        public static void LoadCacheIfNotExist(Action callback = null)
        {
            if (cache == null)
            {
                cache = new APCache();
                if (!LoadFromLocal())
                {
                    CacheManager.LoadDataInToCache(callback);
                }
                else
                {
                    cache.ReBuildMap();
                }
            }
        }

        public static void LoadDataInToCache(Action callback)
        {
            if (isBuldingCache)
            {
                return;
            }
            isBuldingCache = true;

            // Create folder to ensure it's exist.
            Directory.CreateDirectory(Application.persistentDataPath);

            if (cache == null)
            {
                cache = new APCache();
            }

            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("A+ Assets Explorer", "Refresh cache operation is not allowed in play mode", "OK");
                return;
            }

            EditorCoroutine.StartCoroutine(LoadDataIntoCacheCoroutine(callback));
        }

        public static bool IsCacheFileExists()
        {
            return File.Exists(ASSETS_PATH);
        }

        public static bool LoadFromLocal()
        {
#if APLUS_DEV
            var now = DateTime.Now;
#endif
            if (cache == null)
            {
                cache = new APCache();
            }
            cache.Clear();

            bool result = LoadFromLocal<APCache>(ASSETS_PATH, ref cache);
            if (result)
            {
                cache.RawToData();
            }

#if APLUS_DEV
            Debug.Log(String.Format("LoadFromLocal cost {0} ms", (DateTime.Now - now).TotalMilliseconds));
#endif
            return result;
        }

        private static bool LoadFromLocal<T>(string filePath, ref T data) where T : class
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(ASSETS_PATH);
                    JsonUtility.FromJsonOverwrite(json, data);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static void SaveToLocal()
        {
#if APLUS_DEV
            var now = DateTime.Now;
#endif
            if (cache != null)
            {
                cache.DataToRaw();
                SaveToLocal(ASSETS_PATH, cache);
            }
#if APLUS_DEV
            Debug.Log(String.Format("SaveToLocal cost {0} ms", (DateTime.Now - now).TotalMilliseconds));
#endif
        }

        private static void SaveToLocal(string filePath, object data)
        {
            var json = JsonUtility.ToJson(data);
            File.WriteAllText(filePath, json);
        }

        private static IEnumerator LoadDataIntoCacheCoroutine(Action callback)
        {
            try
            {
#if APLUS_DEV
                var now = DateTime.Now;
#endif
                var currentScene = EditorSceneManager.GetActiveScene();
                if (currentScene.isDirty)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }

                string currentScenePath = currentScene.path;
                if (!EditorApplication.isPlaying)
                {
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
                }
                cache.Clear();
                float totalCount = ModuleHelper.Categories.Count();
                ShowProcessBar(0);
                for (int i = 0; i < totalCount; i++)
                {
                    ShowProcessBar((i + 1) / totalCount);
                    var cacheItem = ModuleHelper.Categories[i].CreateCacheItem();
                    cacheItem.assets = ModuleHelper.Categories[i].GetAssets();
                    cacheItem.assets.ForEach(cache.AddToMapIfNeeds);
                    cache.cacheItems.Add(cacheItem);
                }
               
                ShowProcessBar(1);

                if (IsShowProcessBarState())
                {
                    Debug.Log("A+ Assets Explorer cache data was created.");
                }

                if (!string.IsNullOrEmpty(currentScenePath))
                {
                    EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
                }
#if APLUS_DEV
                Debug.Log(String.Format("Get chach data cost {0} ms", (DateTime.Now - now).TotalMilliseconds));
#endif
            }
            catch
            {

            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            SaveToLocal();

            if (callback != null)
            {
                callback();
            }

            yield return Resources.UnloadUnusedAssets();
            isBuldingCache = false;
        }

        private static void ShowProcessBar(float progress)
        {
            if (IsShowProcessBarState())
            {
                string tile = "A+ Assets Explorer";
                string info = "A+ is creating cache data...";
                EditorUtility.DisplayProgressBar(tile, info, progress);
            }
        }

        private static bool IsShowProcessBarState()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode
                && !EditorApplication.isPlaying
                && !EditorApplication.isPaused;
        }
    }
}