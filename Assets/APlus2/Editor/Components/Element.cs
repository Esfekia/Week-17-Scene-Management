//  Copyright (c) 2020-present amlovey
//  
using UnityEngine.UIElements;

namespace APlus2
{
    public class Element : VisualElement
    {
        public Element()
        {

        }

        public void Show()
        {
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            this.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }
    }
}