//  Copyright (c) 2020-present amlovey
//  
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace APlus2
{
    public class About : Container
    {
        private string version = "1.1.0";
        private string documentsUrl = "https://www.amlovey.com/assetexplorer2/manual/";
        private string email = "mailto:amlovey@qq.com";
        private string assetStoreLink = "https://assetstore.unity.com/packages/slug/167838?aid=1011lGoJ";

        public About()
        {
            this.style.top = float.NaN;
            this.style.height = 66;
            this.style.borderTopColor = ColorHelper.Parse("#80808080");
            this.style.borderTopWidth = 1;
            this.style.paddingTop = 6;
            this.style.paddingLeft = 6;
            this.style.paddingRight = 6;

            this.RenderVersion();
            this.RenderLinks();
            this.RenderCopyright();
        }

        private void RenderLinks()
        {
            VisualElement line = new VisualElement();
            line.style.marginTop = 5;
            line.style.marginBottom = 5;
            line.style.flexDirection = FlexDirection.Row;
            
            Label doclink = RenderLink("Documents", () =>
            {
                Application.OpenURL(documentsUrl);
            });
            line.Add(doclink);
            doclink.tooltip = "Online documents";
            
            line.Add(RenderLinkDivider());
            
            Label supportLink = RenderLink("Email", () => 
            {
                Application.OpenURL(email);
            });
            line.Add(supportLink);
            supportLink.tooltip = "Send email to publisher to get support";
            
            line.Add(RenderLinkDivider());

            Label reviewLink = RenderLink("Write Review", () =>
            {
                Application.OpenURL(string.Format("{0}?aid=1011lGoJ", assetStoreLink));
            });
            line.Add(reviewLink);
            reviewLink.tooltip = "Give 5 stars and a good review to support publisher";

            this.Add(line);
        }

        private VisualElement RenderLinkDivider()
        {
            VisualElement divider = new VisualElement();
            divider.style.width = 1;
            divider.style.backgroundColor = ColorHelper.Parse(Themes.Current.ForegroundColor);
            divider.style.marginLeft = 4;
            divider.style.marginRight = 4;
            divider.style.marginTop = 2;
            divider.style.marginBottom = 2;
            return divider;
        }

        private Label RenderLink(string text, Action onClick)
        {
            Label link = new Label(text);
            link.AddToClassList("ap-link");
            link.style.unityFontStyleAndWeight = FontStyle.Bold;
            link.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0 && onClick != null)
                {
                    onClick();
                }
            });
            return link;
        }

        private void RenderCopyright()
        {
            Label copyright = new Label("© 2021 Amlovey. All rights reserved.");
            this.Add(copyright);
        }

        private void RenderVersion()
        {
            Label version = new Label(string.Format("Version: {0}", this.version));
            this.Add(version);
        }
    }
}