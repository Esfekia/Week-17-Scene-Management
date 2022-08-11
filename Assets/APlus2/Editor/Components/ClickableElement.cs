//  Copyright (c) 2020-present amlovey
//  
using UnityEngine.UIElements;
using System;

namespace APlus2
{
    public class ClickableElement : Element
    {
        public Action OnClick;

        private Clickable clickable;

        public ClickableElement()
        {
            clickable = new Clickable(OnClicked);
            this.AddManipulator(clickable);
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