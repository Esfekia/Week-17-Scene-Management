//  Copyright (c) 2020-present amlovey
//  
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace APlus2
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Material))]
    public class MaterialEditorOverride : MaterialEditor
    {
        Dictionary<Material, APAsset> map;

        public override void OnEnable()
        {
            base.OnEnable();
            CacheManager.LoadCacheIfNotExist();
            map = new Dictionary<Material, APAsset>();
            foreach (var matObj in targets)
            {
                Material mat = matObj as Material;
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(mat));
                var asset = CacheManager.cache.GetAsset(guid) as APMaterial;
                map.Add(mat, asset);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UpdateAPMaterialIfNeeds();
        }

        private void UpdateAPMaterialIfNeeds()
        {
            bool changed = false;
            foreach (var matObj in targets)
            {
                Material mat = matObj as Material;
                if (map.ContainsKey(mat)) 
                {
                    var apMaterial = map[mat] as APMaterial;
                    if (apMaterial == null || mat == null)
                    {
                        continue;
                    }

                    if (apMaterial.Shader != mat.shader.name)
                    {
                        apMaterial.Shader = mat.shader.name;
                        changed = true;
                    }
                }
            }
            
            if (MainWindow.Instance != null && changed)
            {
                MainWindow.Instance.table.Update();
            }
        }
    }
}