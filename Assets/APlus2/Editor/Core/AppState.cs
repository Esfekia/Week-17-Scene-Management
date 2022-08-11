//  Copyright (c) 2020-present amlovey
//  
using System.Collections.Generic;
using UnityEngine;
using System;

namespace APlus2
{
    [Serializable]
    public class AppState : ScriptableObject
    {
        public List<AssetCacheItem> items;
        public List<string> keys;
        public string selectedMenuKey;
        public List<object> selections;
        public TableDefinitions tableDef;
        public string searchInputText;

        public List<APAsset> inHierachy;

        public List<APAsset> allAssets;

        public bool menuShow;
        public bool changeHeaderLayerShow;

        public AssetCacheItem GetAssetCacheItem(string assetType)
        {
            foreach (var item in items)
            {
                if (item.assetType == assetType)
                {
                    return item;
                }
            }

            return null;
        }

        public void GetAllAssets()
        {
            if (allAssets == null)
            {
                allAssets = new List<APAsset>();
            }

            allAssets.Clear();

            foreach (var item in items)
            {
                allAssets.AddRange(item.assets);   
            }
        }

        public List<APAsset> getCurrentAssetList()
        {
            switch (selectedMenuKey)
            {
                case Constants.ALL_ASSETS_KEY:
                    return allAssets;
                case Constants.IN_HIERACHY_MENU_KEY:
                    return inHierachy;
                default:
                    foreach (var item in items)
                    {
                        if (item.assetType == selectedMenuKey)
                        {
                            return item.assets;
                        }
                    }
                break;
            }

            return null;
        }

        private void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        public void SyncAssetsDataFromCache()
        {
            CacheManager.LoadCacheIfNotExist();
            items = CacheManager.cache.cacheItems;
        }
    }
}
