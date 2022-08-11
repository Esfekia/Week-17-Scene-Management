//  Copyright (c) 2020-present amlovey
//  
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace APlus2
{
    public partial class MainWindow : EditorWindow
    {
        public static MainWindow Instance;
        private static string Title = "A+ Assets Explorer 2";
        private Label titleLabel;
        private Container contentContainer;
        private Overlayer menuLayer;
        private Container statusBar;
        private Label statusBarLabel;
        private NaviMenu naviMenu;
        private SearchInput searchInput;
        private Overlayer changeHeaderLayer;
        private Container dialog;
        private ListView changeHeaderList;
        private IconButton findReferencesButton;
        private DropdownIconButton copyButton;
        private Overview overview;
        private Settings settingsUI;

        internal Table table;

        public AppState state;

        private void LoadDataAndInitUI()
        {
            if (state == null)
            {
                if (CacheManager.IsCacheFileExists())
                {
                    state = CreateInstance<AppState>();
                    state.tableDef = CreateInstance<TableDefinitions>();
                    state.tableDef.Init();
                }
                else
                {
                    CacheManager.LoadCacheIfNotExist(() =>
                    {
                        state = CreateInstance<AppState>();
                        state.tableDef = CreateInstance<TableDefinitions>();
                        state.tableDef.Init();
                        state.SyncAssetsDataFromCache();
                        BuiltUIWithState();
                    });
                }
            }

            state.SyncAssetsDataFromCache();
            BuiltUIWithState();
        }

        private void OnEnable()
        {
            Instance = this;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
            LoadDataAndInitUI();
            this.rootVisualElement.schedule.Execute(() => 
            { 
                bool isPro = EditorGUIUtility.isProSkin;
                if (isPro && Settings.Theme != ThemeType.Professional.ToString())
                {
                    Themes.ChangeTheme(ThemeType.Professional);
                    this.BuiltUIWithState();
                    return;
                }

                if (!isPro && Settings.Theme != ThemeType.Personal.ToString())
                {
                    Themes.ChangeTheme(ThemeType.Personal);
                    this.BuiltUIWithState();
                    return;
                }
            }).Every(2000);
        }

        private void OnDisable()
        {
            CacheManager.SaveToLocal();
            Blacklist.SaveToLocal();
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
        }

        private void OnSceneChanged(Scene old, Scene newScene)
        {
            RefreshHierarchyData();
        }

        private void OnDestroy()
        {
            DestroyImmediate(state);
        }

        public void BuiltUIWithState()
        {
            RenderUI();
            UpdateUIWithState();
        }

        private void UpdateUIWithState()
        {
            if (state == null)
            {
                return;
            }

            if (state.menuShow)
            {
                this.ShowMenuLayer();
            }
            else
            {
                this.HideMenuLayer();
            }

            this.UpdateUIToSelectedMenu();
        }

        private void UpdateUIToSelectedMenu()
        {
            this.naviMenu.RenderUI(state.selectedMenuKey);
            if (string.IsNullOrEmpty(state.selectedMenuKey))
            {
                state.selectedMenuKey = Constants.OVERVIEW_MENU_KEY;
            }

            foreach (var menu in this.naviMenu.menuItems)
            {
                if (menu.key == state.selectedMenuKey)
                {
                    this.OnNaviMenuClick(menu);
                    break;
                }
            }
        }

        private void RenderUI()
        {
            var stylesheets = Utilities.GetStylesheet();
            if (stylesheets == null)
            {
                return;
            }

            this.rootVisualElement.ClearClassList();
            this.rootVisualElement.Clear();
            this.rootVisualElement.styleSheets.Add(stylesheets);
            this.rootVisualElement.style.backgroundColor = ColorHelper.Parse(Themes.Current.BackgroundColor);
            this.rootVisualElement.style.color = ColorHelper.Parse(Themes.Current.ForegroundColor);
            this.rootVisualElement.EnableInClassList(Settings.Theme, true);

            RenderMenuButton();
            RenderTitle();
            RenderContentContainer();
            RenderStautsBar();
            RenderMenuLayer();
            RenderChangeHeaderLayer();
        }

        private void RenderOverview()
        {
            this.statusBarLabel.text = "";
            this.contentContainer.Clear();
            overview = new Overview(state);
            overview.onItemClick = OnOverviewItemClick;
            this.contentContainer.Add(overview);
        }

        private void OnOverviewItemClick(string category)
        {
            var menu = naviMenu.menuItems.FirstOrDefault(m => m.text == category);
            if (menu != null)
            {
                NavigateTo(menu.key);
                UpdateUIToSelectedMenu();
            }
        }

        private void NavigateToOtherByType(string type)
        {
            var ids = AssetsHelper.GetAssetGuidsByType(type);
            Instance.state.searchInputText = string.Format("Id:{0}", string.Join("|", ids));
            Instance.state.selectedMenuKey = AssetType.OTHERS;
            Instance.UpdateUIToSelectedMenu();
            Instance.DoFilter(Instance.state.searchInputText);
        }

        private void RenderChangeHeaderLayer()
        {
            changeHeaderLayer = new Overlayer();
            dialog = new Container();
            dialog.style.backgroundColor = ColorHelper.Parse(Themes.Current.DialogBackgroundColor);
            this.UpdateDiloagLayout();

            changeHeaderLayer.Add(dialog);
            this.rootVisualElement.Add(changeHeaderLayer);
            changeHeaderLayer.OnClick = () =>
            {
                changeHeaderLayer.Hide();
                dialog.Clear();
                if (table != null)
                {
                    this.table.Update();
                }
            };

            if (!state.changeHeaderLayerShow)
            {
                changeHeaderLayer.Hide();
            }
        }

        private void CreateChangeHeaderList(List<Column> columns)
        {
            dialog.Clear();
            Label title = new Label();
            title.text = "Headers Settings For " + this.titleLabel.text;
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginLeft = 8;
            title.style.marginRight = 8;
            title.style.marginTop = 8;
            title.style.height = 28;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.borderBottomWidth = 1;
            dialog.Add(title);

            var changeHeaderList = new ListView();
            changeHeaderList.style.position = Position.Absolute;
            changeHeaderList.style.top = 42;
            changeHeaderList.style.left = 8;
            changeHeaderList.style.right = 8;
            changeHeaderList.style.bottom = 8;

            changeHeaderList.SetItemHeight(24);
            changeHeaderList.makeItem = () =>
            {
                var toggle = new Toggle();
                toggle.RegisterValueChangedCallback(evt =>
                {
                    var header = toggle.text;
                    foreach (var item in columns)
                    {
                        item.showSortIcon = false;
                        if (item.header == header)
                        {
                            item.visible = evt.newValue;
                        }
                    }
                });
                return toggle;
            };

            changeHeaderList.bindItem = (element, index) =>
            {
                var toggle = element as Toggle;
                if (index < columns.Count())
                {
                    toggle.text = columns[index].header;
                    toggle.value = columns[index].visible;
                    toggle.SetEnabled(index != 0);
                }
            };
            changeHeaderList.itemsSource = columns;
            dialog.Add(changeHeaderList);
        }

        private void UpdateDiloagLayout()
        {
            dialog.style.width = this.position.width * 0.6f;
            dialog.style.height = this.position.height * 0.6f;
            dialog.style.left = this.position.width * 0.2f;
            dialog.style.top = this.position.height * 0.2f;
        }

        private void RenderStautsBar()
        {
            statusBar = new Container();
            statusBar.style.top = float.NaN;
            statusBar.style.height = 24;
            statusBar.style.bottom = 0;
            statusBar.style.right = 0;
            statusBar.style.left = 0;
            statusBar.style.backgroundColor = ColorHelper.Parse(Themes.Current.StatusBarBackgroundColor);
            statusBar.style.paddingLeft = 20;
            statusBar.style.flexDirection = FlexDirection.Row;

            statusBarLabel = UIHelper.CreateNoMarginPaddingLabel();
            statusBarLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

            statusBar.Add(statusBarLabel);
            this.rootVisualElement.Add(statusBar);
        }

        private void RenderContentContainer()
        {
            contentContainer = new Container();
            contentContainer.style.top = 60;
            contentContainer.style.left = 20;
            contentContainer.style.right = 10;
            contentContainer.style.bottom = 28;
            this.rootVisualElement.Add(contentContainer);
        }

        private void updateStatusBar(int count, int selected)
        {
            this.statusBarLabel.text = string.Format("{0} Items, {1} Selected", count, selected);
        }

        private void RenderTable(List<Column> columns, List<APAsset> dataSource, Action<List<APAsset>> OnSorted)
        {
            this.contentContainer.Clear();
            this.statusBar.Show();

            table = new Table(columns);
            table.style.top = 36;
            table.SetDataSource(dataSource.Where(IsNotInBlacklist).ToList());

#if !UNITY_2020_1_OR_NEWER
            table.listView.onItemChosen += item =>
            {
#else
            table.listView.onItemsChosen += (items) => 
            {
                if (items.Count() == 0)
                {
                    return;
                }
                
                var item = items.ElementAt(0);
#endif
                var asset = item as APAsset;
                if (asset != null)
                {
                    PingAsset(asset.Id);
                }
            };

#if UNITY_2020_1_OR_NEWER
            table.listView.onSelectionChange += objects =>
#else
            table.listView.onSelectionChanged += objects =>
#endif
            {
                state.selections = objects.ToList();
                this.updateStatusBar(table.GetItemsCount(), objects.Count());
                UpdateFindReferenceButtonState();
                UpdateCopyButtonState();
            };

            table.OnHeaderClick = column =>
            {
                var ds = column.DoSort(dataSource);
                table.SetDataSource(ds.Where(IsNotInBlacklist).ToList());
                table.columns.ForEach(c => c.showSortIcon = false);
                column.showSortIcon = true;
                table.Update();

                if (OnSorted != null)
                {
                    OnSorted(ds);
                }

                this.DoFilter(state.searchInputText);
            };

            DoFilter(state.searchInputText);
            RenderSearchInput();
            RenderTableToolbar();
            this.updateStatusBar(table.GetItemsCount(), 0);
            contentContainer.Add(table);
        }

        private bool IsNotInBlacklist(APAsset asset)
        {
            if (asset == null || string.IsNullOrEmpty(asset.Path))
            {
                return true;
            }

            if (Blacklist.Cache.items.Any(item => {
                var normalizedPath1 = Utilities.PathNormalized(asset.Path).ToLower();
                var normalizedPath2 = Utilities.PathNormalized(item.ToLower()).ToLower();
                return normalizedPath1.ToLower().StartsWith(normalizedPath2.ToLower());
            }))
            {
                return false;
            }

            return true;
        }

        private void RenderSearchInput()
        {
            searchInput = new SearchInput("Search Assets...");
            searchInput.style.marginTop = 1;
            searchInput.style.maxWidth = 400;
            searchInput.style.minWidth = 140;
            searchInput.SetText(state.searchInputText);
            searchInput.OnSubmit = DoFilter;

            this.contentContainer.Add(searchInput);
        }

        private void DoFilter(string filters)
        {
            state.searchInputText = filters;
            var list = state.getCurrentAssetList();
            if (string.IsNullOrEmpty(filters))
            {
                if (list != null)
                {
                    table.SetDataSource(list.Where(IsNotInBlacklist).ToList());
                    table.Update();
                    table.listView.ClearSelectionRows();
                    this.updateStatusBar(table.GetItemsCount(), 0);
                }
                return;
            }

            var conditions = SearchCondition.Parse(filters);
            list = list.Where(asset =>
            {
                bool include = true;
                foreach (var condition in conditions)
                {
                    include = include && condition.Execute(asset);
                }
                return include;
            }).ToList();

            table.SetDataSource(list.Where(IsNotInBlacklist).ToList());
            table.Update();
            table.listView.ClearSelectionRows();
            this.updateStatusBar(table.GetItemsCount(), 0);
        }

        private void RenderTableToolbar()
        {
            var tableToolBar = new Container();
            tableToolBar.style.height = 28;
            tableToolBar.style.left = 436;
            tableToolBar.style.flexDirection = FlexDirection.Row;
            tableToolBar.style.alignItems = Align.Center;

            IconButton columnsButton = new IconButton(Icons.Columns);
            columnsButton.style.marginRight = 8;
            columnsButton.tooltip = "Change table headers";
            columnsButton.OnClick = () =>
            {
                UpdateDiloagLayout();
                this.CreateChangeHeaderList(this.table.columns);
                changeHeaderLayer.Show();
            };

            IconButton pingButton = new IconButton(Icons.Location);
            pingButton.style.marginRight = 8;
            pingButton.tooltip = "Locate Assets In Project window";
            pingButton.OnClick = PingSelectedAssets;

            IconButton removeButton = new IconButton(Icons.Delete);
            removeButton.tooltip = "Remove selected assets";
            removeButton.OnClick = DeleteAsset;

            copyButton = RenderCopyMenu();
            copyButton.style.marginRight = 8;

            if (state.selectedMenuKey == Constants.IN_HIERACHY_MENU_KEY)
            {
                IconButton refrehDataButton = new IconButton(Icons.Refresh);
                refrehDataButton.style.marginRight = 8;
                refrehDataButton.tooltip = "Refresh table data";
                refrehDataButton.OnClick = RefreshHierarchyData;
                tableToolBar.Add(refrehDataButton);

                findReferencesButton = new IconButton(Icons.List);
                findReferencesButton.style.marginRight = 8;
                findReferencesButton.tooltip = "Find References in Hierarchy";
                findReferencesButton.OnClick = FindReference;
                tableToolBar.Add(findReferencesButton);
            }

            tableToolBar.Add(copyButton);
            tableToolBar.Add(columnsButton);
            tableToolBar.Add(pingButton);

            if (state.selectedMenuKey != Constants.IN_HIERACHY_MENU_KEY)
            {
                tableToolBar.Add(removeButton);
            }

            this.contentContainer.Add(tableToolBar);
        }

        private DropdownIconButton RenderCopyMenu()
        {
            var popup = new DropdownIconButton(Icons.Copy);
            popup.tooltip = "Copy";
            popup.menu.AppendAction("Copy Name To Clipboard", CopyName, a => DropdownMenuAction.Status.Normal);
            popup.menu.AppendAction("Copy Path To Clipboard", CopyPath, a => DropdownMenuAction.Status.Normal);
            return popup;
        }

        private void UpdateCopyButtonState()
        {
            if (copyButton != null)
            {
                var enable = state.selections != null && state.selections.Count() > 0;
                copyButton.SetEnabled(enable);
            }
        }

        private void UpdateFindReferenceButtonState()
        {
            if (findReferencesButton != null)
            {
                var enable = state.selections != null && state.selections.Count() == 1;
                findReferencesButton.SetEnabled(enable);
            }
        }

        private void FindReference()
        {
            Utilities.DebugLog("Find References in Hierarchy");

            var assets = state.selections.Cast<APAsset>();
            if (assets.Count() == 0)
            {
                return;
            }

            SearchableEditorWindow hierarchyWindow = null;
            SearchableEditorWindow[] windows = (SearchableEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(SearchableEditorWindow));
            foreach (SearchableEditorWindow window in windows)
            {
                if (window.GetType().ToString() == "UnityEditor.SceneHierarchyWindow")
                {
                    hierarchyWindow = window;
                    break;
                }
            }

            if (hierarchyWindow == null)
            {
                return;
            }

            var guid = Utilities.GetGuidFromAssetId(assets.ElementAt(0).Id);
            var instanceId = Utilities.GetInstanceIdFromAssetId(assets.ElementAt(0).Id);
            if (!string.IsNullOrEmpty(instanceId))
            {
                instanceId += ":";
            }

            string filter = string.Format("ref:{0}\"{1}\"", instanceId, AssetDatabase.GUIDToAssetPath(guid).Substring(7));

            var setSearchType = typeof(SearchableEditorWindow).GetMethod("SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                setSearchType.Invoke(hierarchyWindow, new object[] { filter, 0, false });
            }
            catch
            {
                setSearchType.Invoke(hierarchyWindow, new object[] { filter, 0, false, false });
            }
        }

        private void RefreshHierarchyData()
        {
            if (state.selectedMenuKey == Constants.IN_HIERACHY_MENU_KEY)
            {
                state.inHierachy = AssetsHelper.GetHierarchyAssets();
                this.RenderTable(
                    state.tableDef.GetAssetColumns(Constants.IN_HIERACHY_MENU_KEY),
                    state.inHierachy,
                    list => state.inHierachy = list);
            }
        }

        private void RenderTitle()
        {
            titleLabel = new Label("OVERVIEW");
            titleLabel.style.position = Position.Absolute;
            titleLabel.style.fontSize = 26;
            titleLabel.style.marginLeft = 70;
            titleLabel.style.marginTop = 16;
            this.rootVisualElement.Add(titleLabel);
        }

        private void SetTitleLabel(string title)
        {
            this.titleLabel.text = title;
        }

        private void RenderMenuButton()
        {
            var menuButton = new IconButton(Icons.Menu);
            menuButton.OnClick = ShowMenuLayer;
            menuButton.style.position = Position.Absolute;
            menuButton.style.marginLeft = 20;
            menuButton.style.marginTop = 20;
            this.rootVisualElement.Add(menuButton);
        }

        private Container RenderSideMenuPanel()
        {
            naviMenu = RenderNaviMenu();
            var container = new Container();
            container.style.width = 240;
            container.style.backgroundColor = ColorHelper.Parse(Themes.Current.DialogBackgroundColor);
            
            naviMenu = RenderNaviMenu();
            ScrollView scrollView = new ScrollView();
            scrollView.style.position = Position.Absolute;
            scrollView.style.top = 20;
            scrollView.style.left = 0;
            scrollView.style.right = 0;
            scrollView.style.bottom = 66;
            scrollView.SetScrollViewHorizontalScrollVisible(false);
            scrollView.Add(naviMenu);
            container.Add(scrollView);

            About about = new About();
            container.Add(about);

            return container;
        }

        private void RenderMenuLayer()
        {
            menuLayer = new Overlayer();
            menuLayer.OnClick = HideMenuLayer;
            this.ShowMenuLayer();
            this.rootVisualElement.Add(menuLayer);
            menuLayer.Hide();
        }

        private void HideMenuLayer()
        {
            this.menuLayer.Hide();
            this.menuLayer.Clear();
            state.menuShow = false;
        }

        private void ShowMenuLayer()
        {
            menuLayer.Add(RenderSideMenuPanel());
            menuLayer.Show();
            state.menuShow = true;
        }

        private NaviMenu RenderNaviMenu()
        {
            naviMenu = new NaviMenu();
            naviMenu.AddItem(new NaviMenuItemGroup(Constants.OVERVIEW_MENU_KEY, "Overview"));
            naviMenu.AddItem(new NaviMenuItemGroup(Constants.IN_HIERACHY_MENU_KEY, "In Hierarchy"));
            naviMenu.AddItem(new NaviMenuItemGroup("assets", "Assets", false));
            ModuleHelper
                .GetMenus()
                .OrderBy(m => m.order).ToList()
                .ForEach(menu => naviMenu.AddItem(menu));

            naviMenu.AddItem(new NaviMenuItemGroup(Constants.SETTINGS_KEY, "Settings"));

            naviMenu.OnMenuItemClicked = menu =>
            {
                state.searchInputText = string.Empty;
                if (menu.key == Constants.IN_HIERACHY_MENU_KEY)
                {
                    state.inHierachy = AssetsHelper.GetHierarchyAssets();
                }

                OnNaviMenuClick(menu);
            };

            naviMenu.RenderUI(state.selectedMenuKey);

            return naviMenu;
        }

        private void NavigateTo(string assetType)
        {
            var item = naviMenu.menuItems.FirstOrDefault(menu => menu.key == assetType);
            if (item != null)
            {
                OnNaviMenuClick(item);
            }
        }

        private void RenderSettings()
        {
            this.contentContainer.Clear();
            this.statusBarLabel.text = string.Empty;
            settingsUI = new Settings();
            this.contentContainer.Add(settingsUI);
        }

        private void OnNaviMenuClick(NaviMenuItem naviMenuItem)
        {
            state.selectedMenuKey = naviMenuItem.key;
            this.SetTitleLabel(naviMenuItem.text);
            this.HideMenuLayer();

            switch (naviMenuItem.key)
            {
                case Constants.SETTINGS_KEY:
                    this.RenderSettings();
                    break;
                case Constants.IN_HIERACHY_MENU_KEY:
                    this.RenderTable(state.tableDef.GetAssetColumns(naviMenuItem.key), state.inHierachy, list => state.inHierachy = list);
                    break;
                case Constants.OVERVIEW_MENU_KEY:
                    this.RenderOverview();
                    break;
                case Constants.ALL_ASSETS_KEY:
                    state.GetAllAssets();
                    var allColumns = state.tableDef.GetAssetColumns(naviMenuItem.key);
                    allColumns.ForEach(c => c.isDesc = NullableBoolean.Unkonwn);
                    this.RenderTable(allColumns, state.allAssets, list => state.allAssets = list);
                    break;
                default:
                    var cacheItem = state.GetAssetCacheItem(naviMenuItem.key);
                    var columns = state.tableDef.GetAssetColumns(naviMenuItem.key);
                    this.RenderTable(columns, cacheItem.assets, list => cacheItem.assets = list);
                    break;
            }

            state.selections = null;
            UpdateFindReferenceButtonState();
            UpdateCopyButtonState();
        }
    }
}