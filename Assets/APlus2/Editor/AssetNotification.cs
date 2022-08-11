//  Copyright (c) 2020-present amlovey
//  
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace APlus2
{
    public class AssetNotification : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            try
            {
                if (!CacheManager.IsCacheFileExists())
                {
                    return;
                }

                CacheManager.LoadCacheIfNotExist();

                HandleImportedAssets(importedAssets);
                HandleDeletedAssets(deletedAssets);
                HandleMovedAssets(movedAssets, movedFromAssetPaths);

                if (MainWindow.Instance != null && MainWindow.Instance.table != null)
                {
                    MainWindow.Instance.RefreshUIIfNeeds(true, false);
                }

                CacheManager.SaveToLocal();
            }
            catch(Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
        }

        private static void HandleMovedAssets(string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (movedFromAssetPaths.Length == 0)
            {
                return;
            }

            for (var i = 0; i < movedAssets.Length; i++)
            {
                Utilities.DebugLog(string.Format("moved {0} to {1}", movedFromAssetPaths[i], movedAssets[i]));
                var sid = AssetDatabase.AssetPathToGUID(movedAssets[i]);
                if (Utilities.IsModelPath(movedAssets[i]))
                {
                    var clipIds = AssetsHelper.GetAnimationClipAssetIdInModel(movedAssets[i]);
                    foreach (var id in clipIds)
                    {
                        CacheManager.cache.MoveTo(sid, movedAssets[i]);
                    }
                }
                else
                {
                    CacheManager.cache.MoveTo(sid, movedAssets[i]);
                }
            }
        }

        private static void HandleDeletedAssets(string[] deletedAssets)
        {
            if (deletedAssets.Length == 0)
            {
                return;
            }

            var list = CacheManager.cache.GetAssetsListByType(AssetType.ANIMATIONS);
            if (list == null)
            {
                return;
            }

            var animationClipIds = list.Select(ani => ani.Id).ToArray();

            foreach (var assetPath in deletedAssets)
            {
                Utilities.DebugLog(string.Format("Deleted: {0}", assetPath));

                if (Utilities.IsModelPath(assetPath))
                {
                    for (int i = 0; i < animationClipIds.Length; i++)
                    {
                        var clip = CacheManager.cache.GetAsset(animationClipIds[i]);
                        if (clip.Path.Contains(assetPath))
                        {
                            CacheManager.cache.RemoveAsset(animationClipIds[i], AssetType.ANIMATIONS);
                        }
                    }
                }

                var id = AssetDatabase.AssetPathToGUID(assetPath);
                APAsset asset;
                if (CacheManager.cache.map.TryGetValue(id, out asset))
                {
                    CacheManager.cache.RemoveAsset(id);
                }
            }
        }

        private static void HandleImportedAssets(string[] importedAssets)
        {
            foreach (var assetPath in importedAssets)
            {
                var id = AssetDatabase.AssetPathToGUID(assetPath);

                if (!CacheManager.cache.HasAsset(id))
                {
                    AddNewImportAssets(assetPath);
                }
                else
                {
                    UpdateReimportExistAssets(assetPath);
                }
            }
        }

        private static void UpdateReimportExistAssets(string assetPath)
        {
            Utilities.DebugLog(string.Format("Update: {0}", assetPath));
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));

            foreach (var category in ModuleHelper.Categories)
            {
                if (category.IsMatch(obj))
                {
                    category.UpdateAsset(assetPath);
                    return;
                }
            }
        }

        private static void AddNewImportAssets(string assetPath)
        {
            Utilities.DebugLog(string.Format("New: {0}", assetPath));
            if (!File.Exists(assetPath) && Directory.Exists(assetPath))
            {
                return;
            }

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            foreach (var category in ModuleHelper.Categories)
            {
                if (category.IsMatch(obj))
                {
                    category.AddAsset(assetPath);
                    return;
                }
            }

            Utilities.DebugLog("New object type = " + obj);
        }
    }
}