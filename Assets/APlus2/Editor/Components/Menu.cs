//  Copyright (c) 2020-present amlovey
//  
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace APlus2
{
    public class NaviMenu : VisualElement
    {
        public Action<NaviMenuItem> OnMenuItemClicked;
        public List<NaviMenuItem> menuItems;
        
        public NaviMenu()
        {
            this.style.flexDirection = FlexDirection.Column;
            menuItems = new List<NaviMenuItem>();
        }

        public void AddItem(NaviMenuItem item)
        {
            this.menuItems.Add(item);
        }

        public void RenderUI(string selectedMenuKey)
        {
            this.Clear();
            foreach (var item in menuItems)
            {
                if (item is NaviMenuItemGroup) 
                {
                    var naviItem = new NaviMenuItemGroupControl(item as NaviMenuItemGroup);
                    naviItem.OnClick = () => {
                        if (item.clickable) 
                        {
                            _OnMenuItemClicked(naviItem);
                        }
                    };
                    this.Add(naviItem);
                }
                else
                {
                    var naviItem = new NaviMenuItemControl(item);
                    naviItem.OnClick = () => {
                        if (item.clickable)
                        {
                            _OnMenuItemClicked(naviItem);
                        }
                    };
                    this.Add(naviItem);
                }
            }

            // this.style.height = GetMenuHeight();
            this.SetSelectedMenu(selectedMenuKey);
        }

        private void SetControlToActive(NaviMenuItemControl control)
        {
            if (control == null)
            {
                return;
            }

            string className = "selected";
            foreach (var child in this.Children())
            {
                child.RemoveFromClassList(className);
            }

            control.AddToClassList(className);
        }

        public void SetSelectedMenu(string key)
        {
            var children = this.Children().Select(c => c as NaviMenuItemControl).ToArray();
            if (string.IsNullOrEmpty(key) && this.childCount > 0)
            {
                var control = children[0];
                SetControlToActive(control);
                return;
            }

            foreach (var item in children)
            {
                if (item.item.key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    SetControlToActive(item);
                    return;
                }
            }
        }

        private void _OnMenuItemClicked(NaviMenuItemControl control)
        {
            this.SetControlToActive(control);
            if (OnMenuItemClicked != null)
            {
                OnMenuItemClicked(control.item);
            }
        }
    }

    public class NaviMenuItemControl : ClickableElement
    {
        public NaviMenuItem item;

        public NaviMenuItemControl(NaviMenuItem item)
        {
            if (item.clickable)
            {
                this.AddToClassList("ap-navi-menuitem");
            }

            this.item = item;
            this.style.fontSize = 13;
            this.style.paddingLeft = 36;
            this.style.paddingRight = 10;
            this.style.paddingTop = 3;
            this.style.paddingBottom = 3;
            this.style.flexDirection = FlexDirection.Row;
            this.style.flexShrink = 0;

            if (item.icon != null)
            {
                var icon = new Label(item.icon.value);
                icon.AddToClassList("ap-icon");

#if UNITY_2021_2_OR_NEWER
                var stylesheet = Utilities.GetStylesheet();
                var path = UnityEditor.AssetDatabase.GetAssetPath(stylesheet);
                var fontAssetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "fontawesome.asset");
                var fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextCore.Text.FontAsset>(fontAssetPath);
                icon.style.unityFontDefinition = new StyleFontDefinition(fontAsset);
#endif
                icon.style.width = 16;
                icon.style.height = 16;
                icon.style.marginTop = 2;
                icon.style.marginRight = 6;
                icon.style.unityTextAlign = TextAnchor.MiddleCenter;
                this.Add(icon);
            }

            var label = new Label(item.text);
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            this.Add(label);
        }
    }

    public class NaviMenuItemGroupControl : NaviMenuItemControl
    {
        public NaviMenuItemGroupControl(NaviMenuItemGroup item) : base(item)
        {
            this.style.fontSize = 16;
            this.style.paddingLeft = 16;
        }
    }

    public class NaviMenuItem
    {
        public bool clickable;
        public string key;
        public string text;
        public Icon icon;
        public int order;

        public NaviMenuItem(Icon icon, string key, string text, int order = 20, bool clickable = true)
        {
            this.key = key;
            this.text = text;
            this.clickable = clickable;
            this.icon = icon;
            this.order = order;
        }

        public NaviMenuItem(string key, string text, bool clickable)
            : this(null, key, text, 10, clickable) 
        {

        }
    }

    public class NaviMenuItemGroup : NaviMenuItem
    {
        public NaviMenuItemGroup(string key, string text, bool clickable = true) : base(null, key, text, 1, clickable)
        {

        }
    }
}