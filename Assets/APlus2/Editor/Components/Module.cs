//  Copyright (c) 2020-present amlovey
//  
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace APlus2
{
    public class AssetTableDefinition : ScriptableObject
    {
        public string assetType;
        public List<Column> columns;

        private void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        public static AssetTableDefinition CreateInstance(string assetType, List<Column> columns)
        {
            var instance = ScriptableObject.CreateInstance<AssetTableDefinition>();
            instance.assetType = assetType;
            instance.columns = columns;
            return instance;
        }
    }

    public abstract class AssetCategory
    {
        public int ExecutionOrder;
        public AssetCategory(int executionOrder = 10)
        {
            this.ExecutionOrder = executionOrder;
        }

        protected long GetTotalFileSize(List<APAsset> assets)
        {
            if (assets == null || assets.Count() == 0)
            {
                return 0;
            }

            long total = 0;
            assets.ForEach(asset => total += asset == null ? 0 : asset.FileSize);
            return total;
        }

        protected long GetUsedStorageSize<T>(List<APAsset> assets, Func<T, long> getter) where T: APAsset
        {
            if (assets == null || assets.Count() == 0)
            {
                return 0;
            }

            long total = 0;
            assets.ForEach(asset => total += asset != null && asset.Used == NullableBoolean.True ? getter(asset as T) : 0);
            return total;
        }

        protected long GetStorageSize<T>(List<APAsset> assets, Func<T, long> getter) where T: APAsset
        {
            if (assets == null || assets.Count() == 0)
            {
                return 0;
            }

            long total = 0;
            assets.ForEach(asset => total += asset == null ? 0 : getter(asset as T));
            return total;
        }

        protected APOverviewItem CreateItem(int number, long fileSize, long appUseSize, long? storageSize = null)
        {
            var instance = new APOverviewItem();
            instance.Assets = CreateMenu().text;
            instance.Number = number;
            instance.FileSize = fileSize;
            if (storageSize.HasValue)
            {
                instance.StorageSize = storageSize.Value;
            }
            else
            {
                instance.StorageSize = fileSize;
            }
            instance.AppUseSize = appUseSize;
            return instance;
        }

        protected void AddAsset<T>(string assetPath, Func<string, T> getAssetByIdFunction) where T : APAsset
        {
            var id = AssetDatabase.AssetPathToGUID(assetPath);
            var asset = getAssetByIdFunction(id);
            CacheManager.cache.AddAsset(asset);
        }

        protected void UpdateAsset<T>(string assetPath, Func<string, T> getAssetByIdFunction) where T : APAsset
        {
            var id = AssetDatabase.AssetPathToGUID(assetPath);
            var asset = getAssetByIdFunction(id);
            CacheManager.cache.UpdateAsset(asset);
        }

        public AssetCacheItem CreateCacheItem()
        {
            return AssetCacheItem.CreateInstanceX(this.CreateAssetType());
        }
        
        public abstract APOverviewItem CreateOverviewItem(AppState state);
        public abstract Type RegisterAPAssetClass();
        public abstract List<APAsset> GetAssets();
        public abstract bool IsMatch(UnityEngine.Object obj);
        public abstract void UpdateAsset(string assetPath);
        public abstract void AddAsset(string assetPath);
        public abstract string CreateAssetType();
        public abstract NaviMenuItem CreateMenu();
        public abstract List<Column> CreateColumns();
        public abstract Dictionary<string, ColumnAction> CreateColumnActions();
    }

    internal class ModuleHelper
    {
        public static List<AssetCategory> Categories = GetCategories();
        private static List<AssetCategory> GetCategories()
        {
            List<AssetCategory> list = new List<AssetCategory>();
            ForInTypeDo<AssetCategory>(cat => list.Add(cat));
            return list.OrderBy(cat => cat.ExecutionOrder).ToList();
        }

        private static Dictionary<Type, string> AssetTypeClassToStringMap;
        public static string GetAssetType(APAsset asset)
        {
            if (AssetTypeClassToStringMap == null)
            {
                AssetTypeClassToStringMap = new Dictionary<Type, string>();
                foreach (var item in Categories)
                {
                    var t = item.RegisterAPAssetClass();
                    if (!AssetTypeClassToStringMap.ContainsKey(t))
                    {
                        AssetTypeClassToStringMap.Add(item.RegisterAPAssetClass(), item.CreateAssetType());
                    }
                }
            }

            var type = asset.GetType();
            if (AssetTypeClassToStringMap.ContainsKey(type))
            {
                return AssetTypeClassToStringMap[type];
            }

            return string.Empty;
        }

        private static Dictionary<string, Type> StringToAssetTypeClassMap;
        public static Type GetAssetTypeClass(string assetType)
        {
            if (StringToAssetTypeClassMap == null)
            {
                StringToAssetTypeClassMap = new Dictionary<string, Type>();
                foreach (var item in Categories)
                {
                    if (!StringToAssetTypeClassMap.ContainsKey(item.CreateAssetType()))
                    {
                        StringToAssetTypeClassMap.Add(item.CreateAssetType(), item.RegisterAPAssetClass());
                    }
                }
            }

            if (StringToAssetTypeClassMap.ContainsKey(assetType))
            {
                return StringToAssetTypeClassMap[assetType];
            }

            return null;
        }

        public static List<Dictionary<string, ColumnAction>> GetAssetsColumnActions()
        {
            return Categories.Select(cat => cat.CreateColumnActions()).ToList();
        }

        public static List<AssetTableDefinition> GetAssetsTD()
        {
            return Categories.Select(cat => AssetTableDefinition.CreateInstance(cat.CreateAssetType(), cat.CreateColumns())).ToList();
        }

        public static List<NaviMenuItem> GetMenus()
        {
            var list = new List<NaviMenuItem>();
            list.Add(new NaviMenuItem(Icons.Asset, Constants.ALL_ASSETS_KEY, "All", 1));
            list.AddRange(Categories.Select(cat => cat.CreateMenu()));
            return list;
        }

        private static void ForInTypeDo<T>(Action<T> callback)
        {
#if APLUS_DEV
            var now = DateTime.Now;
#endif
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assemblies)
            {
                if (item.GetName().Name.Equals("Assembly-CSharp-Editor", StringComparison.OrdinalIgnoreCase)
                    || item.GetName().Name.Equals("APlus2"))
                {
                    var types = item.GetTypes();
                    foreach (Type t in types)
                    {
                        if (t.IsSubclassOf(typeof(T)))
                        {
                            var instance = Activator.CreateInstance(t);
                            if (instance != null)
                            {
                                callback((T)instance);
                            }
                        }
                    }
                }
            }
#if APLUS_DEV
            Debug.Log(String.Format("Cost {0} ms for callback {2} in finding {1} in Assemblies", (DateTime.Now - now).TotalMilliseconds, typeof(T), callback.Method.Name));
#endif
        }
    }
}