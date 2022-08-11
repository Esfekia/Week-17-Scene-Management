//  Copyright (c) 2020-present amlovey
//  
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEngine.UIElements;
using System.IO;

namespace APlus2
{
    public partial class MainWindow
    {
        [MenuItem("Tools/A+ Assets Explorer 2/Main Window", false, 0)]
        public static void LoadWindow()
        {
            Type[] desiredDockNextTo = new Type[] { typeof(SceneView) };
            Instance = GetWindow<MainWindow>(Title, desiredDockNextTo);
        }

        [MenuItem("Tools/A+ Assets Explorer 2/Refresh cache", false, 22)]
        public static void RefreshCache()
        {
            CacheManager.LoadDataInToCache(() =>
            {
                if (Instance == null || Instance.table == null)
                {
                    return;
                }

                Instance.state.SyncAssetsDataFromCache();
                Instance.BuiltUIWithState();
                Instance.table.ResetSortState();
            });
        }

        public void UpdateTableOnly()
        {
            if (table != null)
            {
                this.table.Update();
            }
        }

        public void RefreshUIIfNeeds(bool keepSearch = false, bool resetSort = false)
        {
            if (state.selectedMenuKey == Constants.SETTINGS_KEY)
            {
                if (settingsUI != null)
                {
                    settingsUI.UpdateBlacklist();
                }
                return;
            }

            if (state.selectedMenuKey == Constants.OVERVIEW_MENU_KEY)
            {
                this.RenderOverview();
                return;
            }

            var list = state.getCurrentAssetList();
            if (state.selectedMenuKey == Constants.IN_HIERACHY_MENU_KEY)
            {
                Instance.state.inHierachy = AssetsHelper.GetHierarchyAssets();
                list = Instance.state.inHierachy;
            }

            table.SetDataSource(list);
            table.Update();
            if (table.listView.itemsSource != null)
            {
                updateStatusBar(table.GetItemsCount(), 0);
            }

            if (resetSort)
            {
                Instance.table.ResetSortState();
            }

            if (keepSearch)
            {
                DoFilter(state.searchInputText);
            }
        }

        [MenuItem("Assets/Assets Explorer Blacklist/Add", false, 532)]
        public static void AddToBlacklist()
        {
            foreach (var item in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(item);
                Blacklist.Add(path);
            }

            Blacklist.SaveToLocal();

            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.RefreshUIIfNeeds(true);
            }
        }

        [MenuItem("Assets/Assets Explorer Blacklist/Remove", false, 532)]
        public static void RemoveBlacklist()
        {
            foreach (var item in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(item);
                Blacklist.Remove(path);
            }

            Blacklist.SaveToLocal();

            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.RefreshUIIfNeeds(true);
            }
        }

        [MenuItem("Assets/Show In Assets Explorer", false, 532)]
        public static void ShowInAPlus()
        {
            HashSet<string> types = new HashSet<string>();
            List<string> ids = new List<string>();

            foreach (var item in Selection.objects)
            {
                ids.AddRange(GetIdsOfObject(item, types));
            }

            if (Instance == null)
            {
                LoadWindow();
            }

            if (types.Count == 0)
            {
                EditorUtility.DisplayDialog("No files found!", "May be empty folders selected?", "Ok");
                return;
            }

            Instance.state.searchInputText = string.Format("Id:{0}", string.Join("|", ids));
            Instance.state.selectedMenuKey = types.Count > 1 ? "all" : types.ElementAt(0);

            Instance.UpdateUIToSelectedMenu();
            Instance.DoFilter(Instance.state.searchInputText);
            Instance.Focus();
        }

        private static List<string> GetIdsOfObject(UnityEngine.Object o, HashSet<string> types)
        {
            List<string> ids = new List<string>();

            var path = AssetDatabase.GetAssetPath(o);

            if (o is DefaultAsset && Utilities.IsFolder(path))
            {
                var files = Directory.GetFiles(path);
                foreach (var item in files)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item);
                    ids.AddRange(GetIdsOfObject(obj, types));
                    AssetsHelper.UnloadAsset(obj);
                }

                var subFolders = Directory.GetDirectories(path);
                foreach (var item in subFolders)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(item);
                    ids.AddRange(GetIdsOfObject(obj, types));
                    AssetsHelper.UnloadAsset(obj);
                }
            }
            else
            {
                var assetid = AssetDatabase.AssetPathToGUID(path);
                if (o is AnimationClip)
                {
                    if (!path.EndsWith(".anim"))
                    {
                        assetid = Utilities.GetAssetId(assetid, Utilities.GetLocalIndentifierOfObject(o).ToString());
                    }
                }

                if (!string.IsNullOrEmpty(assetid))
                {
                    ids.Add(assetid);
                }
                
                types.Add(AssetsHelper.GetAssetTypeByObject(o));
            }
            
            return ids;
        }

        [MenuItem("Assets/Select Assets in Selection/Animation", false, 533)]
        public static void SelectAnimationsInSelection()
        {
            SelectAssetTypeInSelection(AssetType.ANIMATIONS);
        }

        [MenuItem("Assets/Select Assets in Selection/Audio", false, 533)]
        public static void SelectAudioInSelections()
        {
            SelectAssetTypeInSelection(AssetType.AUDIOS);
        }

        [MenuItem("Assets/Select Assets in Selection/Code", false, 533)]
        public static void SelectCodeInSelection()
        {
            SelectAssetTypeInSelection(AssetType.CODE);
        }

        [MenuItem("Assets/Select Assets in Selection/Font", false, 533)]
        public static void SelectFontsInSelection()
        {
            SelectAssetTypeInSelection(AssetType.FONTS);
        }

        [MenuItem("Assets/Select Assets in Selection/Video", false, 533)]
        public static void SelectMoviesInSelection()
        {
            SelectAssetTypeInSelection(AssetType.VIDEOS);
        }

        [MenuItem("Assets/Select Assets in Selection/Model", false, 533)]
        public static void SelectModelsInSelection()
        {
            SelectAssetTypeInSelection(AssetType.MODELS);
        }

        [MenuItem("Assets/Select Assets in Selection/Material", false, 533)]
        public static void SelectMaterialInSelctions()
        {
            SelectAssetTypeInSelection(AssetType.MATERIALS);
        }

        [MenuItem("Assets/Select Assets in Selection/Prefab", false, 533)]
        public static void SelectPerfabsInSelections()
        {
            SelectAssetTypeInSelection(AssetType.PREFABS);
        }

        [MenuItem("Assets/Select Assets in Selection/Shader", false, 533)]
        public static void SelectShaderInSelection()
        {
            SelectAssetTypeInSelection(AssetType.SHADERS);
        }

        [MenuItem("Assets/Select Assets in Selection/Texture", false, 533)]
        public static void SelectTextuersInSelection()
        {
            SelectAssetTypeInSelection(AssetType.TEXTURES);
        }

        private static void SelectAssetTypeInSelection(string assetType)
        {
            List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
            foreach (var obj in Selection.objects)
            {
                if (assetType.Equals(AssetsHelper.GetAssetTypeByObject(obj), StringComparison.CurrentCultureIgnoreCase))
                {
                    objects.Add(obj);
                }
            }

            Selection.objects = objects.ToArray();
        }

        [MenuItem("Assets/Find References In Project", false, 535)]
        public static void FindReferences()
        {
            CacheManager.LoadCacheIfNotExist();
            HashSet<string> references = new HashSet<string>();
            var objects = Selection.objects;
            var scriptsMap = AssetsHelper.GetMonoScriptsMap();

            bool Cancel = false;
            for (int i = 0; i < objects.Length; i++)
            {
                var path = AssetDatabase.GetAssetPath(objects[i]);
                string message = "Find references of " + path;
                var referencesAssets = CacheManager.cache.GetReferences(path, message, i * 1f / objects.Length, (i + 1) * 1f / objects.Length, ref Cancel);

                foreach (var item in referencesAssets)
                {
                    if (!item.Equals(path, StringComparison.CurrentCultureIgnoreCase))
                    {
                        references.Add(item);
                    }
                }

                if (objects[i] is ScriptableObject || objects[i] is MonoScript)
                {
                    Type msType = null;
                    if (objects[i] is ScriptableObject)
                    {
                        msType = (objects[i] as ScriptableObject).GetType();
                    }

                    if (objects[i] is MonoScript)
                    {
                        msType = (objects[i] as MonoScript).GetClass();
                    }

                    if (msType != null)
                    {
                        msType
                            .GetFields(ReflectionUtils.BIND_FLAGS)
                            .ToList()
                            .ForEach(f =>
                            {
                                if (scriptsMap.ContainsKey(f.FieldType.ToString()))
                                {
                                    references.Add(scriptsMap[f.FieldType.ToString()]);
                                }
                            });
                    }
                }

                if (Cancel)
                {
                    break;
                }

            }

            if (!Cancel)
            {
                SelectReferences(references.ToList());
            }
            else
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void SelectReferences(List<string> references)
        {
            List<UnityEngine.Object> referencesObjects = new List<UnityEngine.Object>();
            foreach (var item in references)
            {
                referencesObjects.Add(AssetDatabase.LoadAssetAtPath(item, typeof(UnityEngine.Object)));
            }

            if (references.Count == 0)
            {
                EditorUtility.DisplayDialog("No References Found!", "The assets have no references in Project!", "OK");
                EditorUtility.ClearProgressBar();
            }
            else
            {
                Selection.objects = referencesObjects.ToArray();
                EditorUtility.ClearProgressBar();
                if (Event.current != null)
                {
                    Event.current.Use();
                }
            }
        }

        private void CopyName(DropdownMenuAction action)
        {
            CopyAssetPropertyToClipboard(asset => asset.Name);
        }

        private void CopyPath(DropdownMenuAction actio)
        {
            CopyAssetPropertyToClipboard(asset => asset.Path);
        }

        private void CopyAssetPropertyToClipboard(Func<APAsset, string> selector)
        {
            if (state.selections == null)
            {
                return;
            }

            var texts = new List<string>();
            foreach (var asset in state.selections)
            {
                if (asset != null)
                {
                    texts.Add(selector(asset as APAsset));
                }
            }

            TextEditor textEditor = new TextEditor();
            textEditor.ReplaceSelection(string.Join("\n", texts.ToArray()));
            textEditor.OnFocus();
            textEditor.Copy();
        }

        private void PingAsset(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                string message = string.Format("Asset not found: {0}", id);
                EditorUtility.DisplayDialog("404: Not found", message, "OK");
                return;
            }

            UnityEngine.Object obj = null;

            if (Utilities.IsSubAsset(id))
            {
                obj = GetAnimationObjectFromModel(id);
            }
            else
            {
                obj = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(id));
            }

            if (obj == null)
            {
                return;
            }

            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }

        private UnityEngine.Object GetAnimationObjectFromModel(string assetid)
        {
            string guid = Utilities.GetGuidFromAssetId(assetid);
            string fileId = Utilities.GetFileIdFromAssetId(assetid);

            Utilities.DebugLog(string.Format("Find Animation in {0} with id {1}", guid, fileId));

            if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(fileId))
            {
                return null;
            }

            var objects = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid));
            Utilities.DebugLog(string.Format("Get {0} items in {1}", objects.Length, assetid));

            foreach (var obj in objects)
            {
                if (obj is AnimationClip && Utilities.GetLocalIndentifierOfObject(obj).ToString() == fileId)
                {
                    objects = null;
                    return obj;
                }
            }

            return null;
        }

        public void PingSelectedAssets()
        {
            if (state.selections == null)
            {
                return;
            }

            var objects = state.selections.Select(item =>
            {
                var apAsset = item as APAsset;
                return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(apAsset.Path);
            });

            Selection.objects = objects.ToArray();
        }

        private void DeleteAsset()
        {
            if (state.selections == null)
            {
                return;
            }

            string title = "Delete selected file?";
            string message = @"- 'Delete' will delete assets permanently.
- 'Backup And Delete' is slow but will backup assets to Application.persistentDataPath.";
            string alt = "Backup And Delete";
            var seletedIndex = EditorUtility.DisplayDialogComplex(title, message, "Delete", "Cancel", alt);

            if (seletedIndex == 0)
            {
                EditorUtility.DisplayProgressBar("Deleting Assets", "Might be slow, be patient :)", 0f);
                var paths = state.selections.Select(asset => (asset as APAsset).Path).ToArray();
                for (int i = 0; i < paths.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Deleting Assets", "Might be slow, be patient :)", i * 1f / paths.Length);
                    var path = paths[i];
                    if (File.Exists(path))
                    {
                        AssetDatabase.DeleteAsset(path);
                    }
                }

                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
            else if (seletedIndex == 2)
            {
                BackupAssets(state.selections.Select(item => (item as APAsset).Id).ToArray());
            }
        }

        private void BackupAssets(string[] ids)
        {
            if (ids == null)
            {
                return;
            }

            List<string> backupfiles = new List<string>();
            for (int i = 0; i < ids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(ids[i]);
                if (File.Exists(path))
                {
                    backupfiles.Add(path);
                }
            }

            string exportFileName = string.Format("AE2_Backup_{0}.unitypackage", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
            var info = Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "AEBackup"));
            exportFileName = Path.Combine(info.FullName, exportFileName);
            EditorUtility.DisplayProgressBar("Creating backup package", "Might be slow, be patient :)", 0.2f);
            AssetDatabase.ExportPackage(backupfiles.ToArray(), exportFileName, ExportPackageOptions.Default);

            for (int i = 0; i < backupfiles.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Creating backup package", "Might be slow, be patient :)", 0.2f + (0.8f * i / backupfiles.Count));
                AssetDatabase.DeleteAsset(backupfiles[i]);
            }

            AssetDatabase.Refresh();
            QuickOpener.Reveal(info.FullName);
            EditorUtility.ClearProgressBar();
        }
    }
}
