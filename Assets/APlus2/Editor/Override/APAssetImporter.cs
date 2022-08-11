//  Copyright (c) 2020-present amlovey
//  
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace APlus2
{
    public class APScriptableObject : ScriptableObject { }

    public abstract class APAssetImporter<T, T1> : Editor where T : APScriptableObject where T1 : APAsset
    {
        Dictionary<T, APAsset> map;

        public virtual void OnEnable()
        {
            CacheManager.LoadCacheIfNotExist();
            map = new Dictionary<T, APAsset>();
            foreach (var target in targets)
            {
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target));
                var asset = CacheManager.cache.GetAsset(guid);
                map.Add(target as T, asset);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UpdateUIIfNeeds();
        }
        
        private void UpdateUIIfNeeds()
        {
            bool changed = false;
            foreach (var target in targets)
            {
                var targetT = target as T;
                if (targetT == null)
                {
                    continue;
                }

                if (map.ContainsKey(targetT))
                {
                    var asset = map[targetT] as T1;
                    if (ShouldUpdate(asset, targetT))
                    {
                        var assetType = ModuleHelper.GetAssetType(asset);
                        var newAsset = TargetToAsset(targetT, asset);
                        CacheManager.cache.UpdateAsset(newAsset);
                        map[targetT] = newAsset;
                        changed = true;
                    }
                }
            }

            if (changed && MainWindow.Instance != null)
            {
                MainWindow.Instance.table.Update();
            }
        }

        protected abstract T1 TargetToAsset(T target, T1 currentAsset);
        protected abstract bool ShouldUpdate(T1 cachItem, T target);
    }
}
