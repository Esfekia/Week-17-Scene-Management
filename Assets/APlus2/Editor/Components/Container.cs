//  Copyright (c) 2020-present amlovey
//  
using UnityEngine.UIElements;

namespace APlus2
{
    public class Container : ClickableElement
    {
        public Container()
        {
            this.style.position = Position.Absolute;
            this.style.left = 0;
            this.style.right = 0;
            this.style.top = 0;
            this.style.bottom = 0;
        }
    }

    public class Overlayer : Container
    {
        public Overlayer()
        {
            this.style.backgroundColor = ColorHelper.Parse("#00000080");
        }
    }
}
