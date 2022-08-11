//  Copyright (c) 2020-present amlovey
//  
using System;
using UnityEngine.UIElements;

namespace APlus2
{
    public class IconButton : Button
    {
        public Action OnClick;

        public IconButton(Icon icon)
        {
            this.style.backgroundImage = null;
            this.AddToClassList("ap-hoverable");
            this.AddToClassList("ap-icon-button");

#if UNITY_2021_2_OR_NEWER
            var stylesheet = Utilities.GetStylesheet();
            var path = UnityEditor.AssetDatabase.GetAssetPath(stylesheet);
            var fontAssetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "fontawesome.asset");
            var fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextCore.Text.FontAsset>(fontAssetPath);
            this.style.unityFontDefinition = new StyleFontDefinition(fontAsset);
#endif
            this.text = icon.value;
            this.clickable.clicked += OnClicked;
        }

        private void OnClicked()
        {
            if (OnClick != null)
            {
                OnClick();
            }
        }
    }

    public class TextButton : Button
    {
        public Action OnClick;

        public TextButton(string text)
        {
            this.style.backgroundImage = null;
            this.AddToClassList("ap-hoverable");
            this.AddToClassList("ap-text-button");
            this.text = text;
            this.clickable.clicked += OnClicked;
        }

        private void OnClicked()
        {
            if (OnClick != null)
            {
                OnClick();
            }
        }
    }
}