//  Copyright (c) 2020-present amlovey
//  
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace APlus2
{
    public class AssetNameElement : Element
    {
        private Image icon;
        private Label label;
        private float width;

        public AssetNameElement(float width)
        {
            this.width = width;
            this.style.paddingLeft = 0;
            this.style.paddingRight = 0;
            this.style.width = width;
            this.style.flexShrink = 0;
            this.style.overflow = Overflow.Hidden;
            this.style.flexDirection = FlexDirection.Row;
            this.style.unityTextAlign = TextAnchor.MiddleLeft;

            icon = new Image();
            icon.style.width = 16;
            icon.style.height = 16;
            icon.style.flexShrink = 0;
            icon.style.marginRight = 10;
            icon.style.alignSelf = Align.Center;

            label = UIHelper.CreateNoMarginPaddingLabel();
            label.style.flexShrink = 0;
            label.style.width = width - 30;

            this.Add(icon);
            this.Add(label);
        }

        private string UpdateTextEllipsisIfNeeds(string value)
        {
            var valMayWithEllipsis = value;
            var maxLength = Mathf.FloorToInt((this.width - 30) / 7);
            if (value.Length > maxLength)
            {
                valMayWithEllipsis = value.Substring(0, maxLength - 3) + "...";
            }

            return valMayWithEllipsis;
        }

        public void SetData(APAsset asset)
        {
            icon.image = AssetDatabase.GetCachedIcon(asset.Path);
            label.text = UpdateTextEllipsisIfNeeds(asset.Name);
            label.tooltip = label.text.Equals(asset.Name) ? null : asset.Name;
        }
    }
}
