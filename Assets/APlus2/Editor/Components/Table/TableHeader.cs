//  Copyright (c) 2020-present amlovey
//  
using UnityEngine;
using UnityEngine.UIElements;

namespace APlus2
{
    public class TableHeader : ClickableElement
    {
        public const int DEFAULT_HEIGHT = 32;
        private Label headerText;
        private Label sortIcon;
        private Column column;

        public TableHeader(Column column)
        {
            this.AddToClassList("ap-table-header");
            this.column = column;
            this.style.fontSize = 14;
            this.style.width = column.width;
            this.style.height = DEFAULT_HEIGHT;
            this.style.flexShrink = 0;
            this.style.flexDirection = FlexDirection.Row;
            this.style.alignItems = Align.Center;
            this.tooltip = column.tooltip;

            this.headerText = new Label(column.header);
            this.headerText.style.left = 0;
            this.headerText.style.right = 0;
            this.headerText.style.top = 0;
            this.headerText.style.height = DEFAULT_HEIGHT;
            this.headerText.style.unityTextAlign = TextAnchor.MiddleLeft;
            this.headerText.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.headerText.style.paddingLeft = 0;
            this.headerText.style.paddingRight = 0;

            sortIcon = new Label();
            sortIcon.AddToClassList("ap-icon");

#if UNITY_2021_2_OR_NEWER
            var stylesheet = Utilities.GetStylesheet();
            var path = UnityEditor.AssetDatabase.GetAssetPath(stylesheet);
            var fontAssetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "fontawesome.asset");
            var fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextCore.Text.FontAsset>(fontAssetPath);
            sortIcon.style.unityFontDefinition = new StyleFontDefinition(fontAsset);
#endif
            sortIcon.style.width = 24;
            sortIcon.style.height = 24;
            sortIcon.style.fontSize = 14;
            sortIcon.style.marginLeft = 5;
            sortIcon.style.color = ColorHelper.Parse(Themes.Current.ForegroundColor);
            sortIcon.style.alignItems = Align.Center;
            sortIcon.style.unityTextAlign = TextAnchor.MiddleLeft;
            
            this.Add(this.headerText);
            this.Add(this.sortIcon);

            this.ShowSortIconIfNeeds();
        }

        public void ShowSortIconIfNeeds()
        {
            if (this.column.showSortIcon)
            {
                sortIcon.visible = true;
                switch (column.isDesc)
                {
                    case NullableBoolean.True:
                        sortIcon.text = Icons.Down.value;
                        break;
                    case NullableBoolean.False:
                        sortIcon.text = Icons.Up.value;
                        break;
                }
            }
            else
            {
                sortIcon.visible = false;
            }
        }

        public void UpdateShowOrHide()
        {
            if (this.column.visible)
            {
                this.Show();
                this.style.width = this.column.width;
            }
            else
            {
                this.Hide();
                this.style.width = 0;
            }
        }
    }
}
