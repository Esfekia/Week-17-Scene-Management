//  Copyright (c) 2020-present amlovey
//  
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace APlus2
{
    public class Settings : Container
    {
        private const string CODE_EXTENSION_KEY = "APUS2_CODE_EXTENSION_KEY";
        public static string CodeFileExtensions
        {
            get
            {
                return EditorPrefs.GetString(CODE_EXTENSION_KEY, Constants.DEFAULT_SCRIPT_EXT);
            }
            set
            {
                EditorPrefs.SetString(CODE_EXTENSION_KEY, value);
            }
        }

        private const string THEME_KEY = "APLUS_THEME";
        public static string Theme
        {
            get
            {
                return EditorPrefs.GetString(THEME_KEY, ThemeType.Personal.ToString());
            }
            set
            {
                EditorPrefs.SetString(THEME_KEY, value);
            }
        }

        private string selectedBlacklist;
        private ListView list;

        public Settings()
        {
            this.Add(RenderGroupText("General"));
            this.RenderCodeExtensionsSettings();
            this.Add(RenderGroupText("Ingore"));
            this.RenderBlacklist();
        }

        private Label RenderGroupText(string text)
        {
            Label label = new Label(text);
            label.style.fontSize = 21;
            label.style.paddingTop = 10;
            label.style.paddingBottom = 10;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            return label;
        }

        private void RenderBlacklist()
        {
            Label intro = CreatePrefixLabel("Below assets will not show in assets table.");
            intro.style.marginBottom = 8;
            this.Add(intro);

            list = new ListView();
            list.selectionType = SelectionType.Single;
            list.style.borderTopWidth = 1;
            list.style.borderBottomWidth = 1;
            list.style.borderTopColor = ColorHelper.Parse("#808080");
            list.style.borderBottomColor = ColorHelper.Parse("#808080");
            list.style.width = 600;
            list.style.minWidth = 240;
            list.style.minHeight = 240;
            list.SetItemHeight(28);
            
            list.makeItem = () =>
            {
                Label label = new Label();
                label.style.fontSize = 12;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                return label;
            };

            var source = Blacklist.Cache.items;
            list.bindItem = (element, index) =>
            {
                var label = element as Label;
                label.text = string.Format("#{0}        {1}", index + 1, source[index]);
            };
#if !UNITY_2020_1_OR_NEWER
            list.onItemChosen += folder =>
            {
#else
            list.onItemsChosen += folders => 
            {
                if (folders.Count() == 0)
                {
                    return;
                }
                
                var folder = folders.ElementAt(0);
#endif
                var obj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder.ToString());
                if (obj)
                {
                    EditorGUIUtility.PingObject(obj);
                }
            };

            TextButton button = new TextButton("Delete Item");
            button.style.marginTop = 6;
            button.style.width = 170;
            button.style.marginLeft = 430;
            button.OnClick = () =>
            {
                Blacklist.Remove(selectedBlacklist);
                list.itemsSource = Blacklist.Cache.items;
                selectedBlacklist = string.Empty;
                button.SetEnabled(false);
            };
            button.SetEnabled(false);

#if !UNITY_2020_1_OR_NEWER
            list.onSelectionChanged += objects => 
#else
            list.onSelectionChange += objects =>
#endif
            {
                selectedBlacklist = objects.ElementAt(0).ToString();
                button.SetEnabled(true);
            };

            list.itemsSource = source;

            this.Add(list);
            this.Add(button);
        }

        public void UpdateBlacklist()
        {
            list.itemsSource = Blacklist.Cache.items;
        }
        
        private DropdownMenuAction.Status IsThemeSelected(ThemeType themeCompareTo)
        {
            return Theme == themeCompareTo.ToString() ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
        }

        private void RenderCodeExtensionsSettings()
        {
            var row = CreateRow();
            row.style.marginBottom = 25;
            row.tooltip = "File extensions for code assets. After changing code file extensions, we have to rebuilt cache to make it available.";

            Label prefix = CreatePrefixLabel("Code File Extensions:");

            TextField inputField = new TextField();
            inputField.value = CodeFileExtensions;
            inputField.style.width = 310;
            inputField.style.height = 21;
            inputField.RegisterValueChangedCallback(evt => CodeFileExtensions = evt.newValue);

            TextButton button = new TextButton("Restore to default");
            button.style.marginTop = -2;
            button.style.marginLeft = 8;
            button.OnClick = () =>
            {
                CodeFileExtensions = Constants.DEFAULT_SCRIPT_EXT;
                inputField.value = CodeFileExtensions;
            };

            row.Add(prefix);
            row.Add(inputField);
            row.Add(button);

            this.Add(row);
        }

        private Label CreatePrefixLabel(string text)
        {
            Label prefix = new Label(text);
            prefix.style.fontSize = 13;
            prefix.style.marginRight = 10;
            prefix.style.width = 140;
            prefix.style.flexShrink = 0;
            prefix.style.flexGrow = 0;
            prefix.style.unityTextAlign = TextAnchor.MiddleLeft;
            return prefix;
        }

        private VisualElement CreateRow()
        {
            var element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;
            element.style.marginBottom = 8;
            return element;
        }
    }
}