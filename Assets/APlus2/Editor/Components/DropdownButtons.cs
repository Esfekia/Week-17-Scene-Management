//  Copyright (c) 2020-present amlovey
//  
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace APlus2
{
    public class DropdownIconButton : TextElement, IToolbarMenuElement
    {
        Clickable clickable;

        public DropdownMenu menu { get; }
        private TextElement arrowElement;

        public DropdownIconButton(Icon icon)
        {
            this.text = icon.value;
            menu = new DropdownMenu();
            clickable = new Clickable(this.ShowMenu);
            this.AddManipulator(clickable);

            this.EnableInClassList("unity-toolbar-menu--popup", true);
            this.AddToClassList("ap-hoverable");
            this.AddToClassList("ap-icon-button");
            this.style.unityTextAlign = TextAnchor.MiddleLeft;
            this.style.marginTop = 3;
            this.style.paddingLeft = 10;
            this.style.width = 48;
            this.style.flexShrink = 0;

#if UNITY_2021_2_OR_NEWER
            var stylesheet = Utilities.GetStylesheet();
            var path = UnityEditor.AssetDatabase.GetAssetPath(stylesheet);
            var fontAssetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "fontawesome.asset");
            var fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextCore.Text.FontAsset>(fontAssetPath);
            this.style.unityFontDefinition = new StyleFontDefinition(fontAsset);
#endif

            arrowElement = new TextElement();
            arrowElement.text = Icons.Down.value;
            arrowElement.style.color = ColorHelper.Parse(Themes.Current.ForegroundColor);
            arrowElement.style.position = Position.Absolute;
            arrowElement.style.top = 0;
            arrowElement.style.bottom = 0;
            arrowElement.style.right = 8;
            arrowElement.style.fontSize = 8;
            arrowElement.style.unityTextAlign = TextAnchor.MiddleCenter;
            arrowElement.pickingMode = PickingMode.Ignore;
            Add(arrowElement);
        }
    }

    public class DropdownTextButton : TextElement, IToolbarMenuElement
    {
        Clickable clickable;

        public DropdownMenu menu { get; }
        private TextElement arrowElement;
        private TextElement textElement;

        public override string text
        {
            get
            {
                return base.text;
            }
            set
            {
                textElement.text = value; 
                base.text = value;
            }
        }

        public DropdownTextButton(string text)
        {
            menu = new DropdownMenu();
            clickable = new Clickable(this.ShowMenu);
            this.AddManipulator(clickable);

            this.AddToClassList("ap-hoverable");
            this.AddToClassList("ap-text-button");
            this.style.flexShrink = 0;
            this.style.flexDirection = FlexDirection.Row;

            textElement = new TextElement();
            textElement.style.color = ColorHelper.Parse(Themes.Current.ForegroundColor);
            textElement.style.fontSize = 14;
            textElement.style.marginRight = 24;
            textElement.pickingMode = PickingMode.Ignore;
            Add(textElement);

            this.text = text;

            arrowElement = new TextElement();
            arrowElement.AddToClassList("ap-icon");
            arrowElement.text = Icons.Down.value;
            arrowElement.style.color = ColorHelper.Parse(Themes.Current.ForegroundColor);
            arrowElement.style.position = Position.Absolute;
            arrowElement.style.top = 0;
            arrowElement.style.bottom = 0;
            arrowElement.style.right = 8;
            arrowElement.style.fontSize = 8;
            arrowElement.style.unityTextAlign = TextAnchor.MiddleCenter;
            arrowElement.pickingMode = PickingMode.Ignore;

#if UNITY_2021_2_OR_NEWER
            var stylesheet = Utilities.GetStylesheet();
            var path = UnityEditor.AssetDatabase.GetAssetPath(stylesheet);
            var fontAssetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "fontawesome.asset");
            var fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextCore.Text.FontAsset>(fontAssetPath);
            arrowElement.style.unityFontDefinition = new StyleFontDefinition(fontAsset);
#endif
            Add(arrowElement);
        }
    }
}