//  Copyright (c) 2020-present amlovey
//  
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;

namespace APlus2
{
    public class Overview : Container
    {
        private AppState state;

        public Action<string> onItemClick;

        public Overview(AppState state)
        {
            this.state = state;
            this.GenerateChildrens();
        }

        public void FindUnusedAssets()
        {
            string title = "Find unused files?";
            string message = "Press 'OK' to launch a build setting dialog to start a build.\r\n\r\n";
            if (EditorUtility.DisplayDialog(title, message, "OK", "Cancel"))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                CacheManager.SaveToLocal();
            }
        }

        public void RebuildCache()
        {
            CacheManager.LoadDataInToCache(()=>
            {
                MainWindow.Instance.RefreshUIIfNeeds();
            });
        }

        public void GenerateChildrens()
        {
            this.Clear();

            var tableToolBar = new Container();
            tableToolBar.style.height = 28;
            tableToolBar.style.flexDirection = FlexDirection.Row;

            TextButton findUnusedButton = new TextButton("Find Unused Assets");
            findUnusedButton.style.width = 180;
            findUnusedButton.style.marginLeft = 0;
            findUnusedButton.OnClick = FindUnusedAssets;
            findUnusedButton.tooltip = "Trigger a build to find unused assets";

            TextButton rebuiltCacheButton = new TextButton("Rebuild Assets Cache");
            rebuiltCacheButton.style.width = 180;
            rebuiltCacheButton.style.marginLeft = 8;
            rebuiltCacheButton.OnClick = RebuildCache;
            rebuiltCacheButton.tooltip = "Rebuild asset cache to get lastest data";

            tableToolBar.Add(findUnusedButton);
            tableToolBar.Add(rebuiltCacheButton);
            this.Add(tableToolBar);

            var columns = state.tableDef.GetAssetColumns(Constants.OVERVIEW_MENU_KEY);
            columns.ForEach(col => col.showSortIcon = false);
            var table = new Table(columns);
            table.style.top = 32;
            table.listView.selectionType = SelectionType.Single;
            var list = GetOverviewItems();
            table.SetDataSource(list.Cast<APAsset>().ToList());
#if !UNITY_2020_1_OR_NEWER
            table.listView.onItemChosen += OnItemDoubleClick;
#else
            table.listView.onItemsChosen += (items) => OnItemDoubleClick(items.ElementAt(0));
#endif
            table.OnHeaderClick += column =>
            {
                var dataSource = column.DoSort(table.listView.itemsSource as List<APAsset>);
                table.SetDataSource(dataSource);
                table.columns.ForEach(c => c.showSortIcon = false);
                column.showSortIcon = true;
                table.Update();
            };

            this.Add(table);
        }

        private void OnItemDoubleClick(object asset)
        {
            var overviewAsset = asset as APOverviewItem;
            if (overviewAsset != null && onItemClick != null)
            {
                onItemClick(overviewAsset.Assets);
            }
        }

        private List<APOverviewItem> GetOverviewItems()
        {
            List<APOverviewItem> list = new List<APOverviewItem>();

            if (state == null)
            {
                return list;
            }

            ModuleHelper.Categories.ForEach(cat => list.Add(cat.CreateOverviewItem(this.state)));
            return list;
        }
    }
}