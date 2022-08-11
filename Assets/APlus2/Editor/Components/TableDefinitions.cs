//  Copyright (c) 2020-present amlovey
//  
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace APlus2
{
    public class TableDefinitions : ScriptableObject
    {
        public List<AssetTableDefinition> assetsTD;
        public List<Column> allColumns;

        private List<Column> CreateAllColumns()
        {
            var columns = new List<Column>();
            columns.Add(Column.CreateInstance("APAsset_Name", "Name", 240));
            columns.Add(Column.CreateInstance("APAsset_FileSize", "FileSize"));
            columns.Add(Column.CreateInstance("APAsset_FileType", "FileType"));
            columns.Add(Column.CreateInstance("APAsset_Used", "Used"));
            columns.Add(Column.CreateInstance("APAsset_Path", "Path", 360, false));
            columns.Add(Column.CreateInstance("APAsset_Id", "Id", 280, false));
            columns.Add(Column.CreateInstance("APAsset_Hash", "Hash", 280, false));
            columns.Add(Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false));


            Dictionary<string, Column> fliterMap = new Dictionary<string, Column>();

            foreach (var item in assetsTD)
            {
                foreach (var col in item.columns)
                {
                    if (col.key.StartsWith("APAsset_"))
                    {
                        continue;
                    }

                    if (fliterMap.ContainsKey(col.header))
                    {
                        var c = fliterMap[col.header];
                        c.additionalKeys = c.additionalKeys.Concat(new string[] { col.key }).ToArray();
                    }
                    else
                    {
                        var colCopy = Column.CreateCopy(col);
                        colCopy.visible = false;
                        colCopy.additionalKeys = new string[] { };
                        fliterMap.Add(colCopy.header, colCopy);
                    }

                }
            }

            columns.AddRange(fliterMap.Values);
            return columns;
        }

        public List<Column> GetAssetColumns(string assetType)
        {
            if (assetType == Constants.ALL_ASSETS_KEY)
            {
                if (allColumns == null)
                {
                    allColumns = CreateAllColumns();
                }

                return allColumns;
            }

            foreach (var td in assetsTD)
            {
                if (td.assetType == assetType)
                {
                    return td.columns;
                }
            }

            return new List<Column>();
        }

        private static Dictionary<string, ColumnAction> BuildActionsMap()
        {
            Dictionary<string, ColumnAction> map = new Dictionary<string, ColumnAction>()
            {
                // Common Actions
                { "#", new IndexColumnAction()},
                { "apasset_name", new AssetNameColumnAction<APAsset>() },
                { "apasset_filesize", new LabelColumnAction<APAsset>(asset => Utilities.GetSizeDescription(asset.FileSize), asset => asset.FileSize)},
                { "apasset_path", new LabelColumnAction<APAsset>(asset => asset.Path, asset => asset.Path)},
                { "apasset_used", new FontIconColumnAction<APAsset>(asset => Utilities.ToUsed(asset.Used))},
                { "apasset_id", new LabelColumnAction<APAsset>(asset => asset.Id, asset => asset.Id)},
                { "apasset_hash", new LabelColumnAction<APAsset>(asset => asset.Hash, asset => asset.Hash)},
                { "apasset_inassetbundle", new FontIconColumnAction<APAsset>(asset => asset.InAssetBundle)},
                { "apasset_filetype", new LabelColumnAction<APAsset>(file => file.FileType, file => file.FileType )},
#if ADDRESSABLE_ON
                { "apasset_inaddressables", new FontIconColumnAction<APAsset>(asset => asset.InAddressables)},
#endif
            };

            ModuleHelper.GetAssetsColumnActions().ForEach(actionsMap =>
            {
                if (actionsMap == null)
                {
                    return;
                }

                foreach (KeyValuePair<string, ColumnAction> item in actionsMap)
                {
                    if (map.ContainsKey(item.Key))
                    {
                        map[item.Key] = item.Value;
                    }
                    else
                    {
                        map.Add(item.Key, item.Value);
                    }
                }
            });

            // Add overviews
            map.Add("apoverviewitem_assets", new LabelColumnAction<APOverviewItem>(item => item.Assets, item => item.Assets));
            map.Add("apoverviewitem_number", new LabelColumnAction<APOverviewItem>(item => item.Number.ToString(), item => item.Number));
            map.Add("apoverviewitem_storagesize", new LabelColumnAction<APOverviewItem>(item => Utilities.GetSizeDescription(item.StorageSize), item => item.StorageSize));
            map.Add("apoverviewitem_appusesize", new LabelColumnAction<APOverviewItem>(item => Utilities.GetSizeDescription(item.AppUseSize), item => item.AppUseSize));

            foreach (KeyValuePair<string, ColumnAction> item in map)
            {
                item.Value.key = item.Key;
            }
            
            return map;
        }

        public static Dictionary<string, ColumnAction> ActionsMap = BuildActionsMap();

        public void Init()
        {
            assetsTD = ModuleHelper.GetAssetsTD();
            assetsTD.Add(AssetTableDefinition.CreateInstance(
                Constants.IN_HIERACHY_MENU_KEY,
                new List<Column>()
                {
                    Column.CreateInstance("APAsset_Name", "Name", 240),
                    Column.CreateInstance("APAsset_FileSize", "FileSize"),
                    Column.CreateInstance("APAsset_FileSize", "FileType"),
                    Column.CreateInstance("APAsset_Path", "Path", 360, false),
                    Column.CreateInstance("APAsset_Id", "Id", 280, false),
                    Column.CreateInstance("APAsset_Hash", "Hash", 280, false),
                    Column.CreateInstance("APAsset_InAssetBundle", "InAssetBundle", 140, false),
#if ADDRESSABLE_ON
                    Column.CreateInstance("APAsset_InAddressables", "InAddressables", 140, false)
#endif
                }
            ));

            assetsTD.Add(AssetTableDefinition.CreateInstance(
                Constants.OVERVIEW_MENU_KEY,
                new List<Column>()
                {
                    Column.CreateInstance("APOverviewItem_Assets", "Assets", 140),
                    Column.CreateInstance("APOverviewItem_Number", "Number"),
                    Column.CreateInstance("APAsset_FileSize", "FileSize", true, "Size of asset files"),
                    Column.CreateInstance("APOverviewItem_StorageSize", "StorageSize", true, "Storage size of asset files with compression"),
                    Column.CreateInstance("APOverviewItem_AppUseSize", "UsedAssetsSize", true, "Size of asset files will be built into apps"),
                }
            ));
        }

        private void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
