using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace APlus2
{
    [Serializable]
    public class BlacklistCache
    {
        public List<string> items;

        public BlacklistCache()
        {
            items = new List<string>();
        }
    }

    public class Blacklist
    {
        private static string CacheFilePath = "ProjectSettings/APlus2Blacklist.asset";
        private static List<string> DefaultList = new List<string>()
        {
            "Packages",
            "Assets/APlus2",
        };

        private static BlacklistCache cache;
        public static BlacklistCache Cache
        {
            get
            {
                if (cache == null)
                {
                    LoadFromLocal();
                }

                return cache;
            }
        }

        public static void Add(string path)
        {
            if (!Cache.items.Contains(path))
            {
                Cache.items.Add(path);
            }
        }

        public static void Remove(string folder)
        {
            if (Cache.items.Contains(folder))
            {
                Cache.items.Remove(folder);
            }
        }

        public static void ResetToDefault()
        {
            Cache.items = DefaultList;
            SaveToLocal();
        }

        public static void LoadFromLocal()
        {
            if (!File.Exists(CacheFilePath))
            {
                cache = new BlacklistCache();
                cache.items = DefaultList;
                SaveToLocal();
            }
            else
            {
                var json = File.ReadAllText(CacheFilePath);
                cache = JsonUtility.FromJson<BlacklistCache>(json);
            }
        }

        public static void SaveToLocal()
        {
            if (cache != null)
            {
                var json = JsonUtility.ToJson(cache);
                File.WriteAllText(CacheFilePath, json);
            }
        }
    }
}