//  Copyright (c) 2020-present amlovey
//  
using UnityEngine.UIElements;

namespace APlus2
{
    public static class UIHelper
    {
        public static Label CreateNoMarginPaddingLabel()
        {
            var label = new Label();
            label.style.paddingLeft = 0;
            label.style.paddingRight = 0;
            label.style.paddingBottom = 0;
            label.style.paddingTop = 0;
            label.style.marginTop = 0;
            label.style.marginBottom = 0;
            label.style.marginLeft = 0;
            label.style.marginRight = 0;
            return label;
        }

        public static Icon GetTrueOrFaseMark(bool value)
        {
            return value ? Icons.True : Icons.False;
        }

        public static Icon GetUnsedMark(bool? used)
        {
            if (used.HasValue)
            {
                return GetTrueOrFaseMark(used.Value);
            }

            return Icons.Question;
        }

        public static void SetItemHeight(this ListView listView, int height)
        {
#if UNITY_2021_2_OR_NEWER
            listView.fixedItemHeight = height;
#else
            listView.itemHeight = height;
#endif
        }

        public static void RebuildView(this ListView listView)
        {
#if UNITY_2021_2_OR_NEWER
            listView.Rebuild();
#else
            listView.Refresh();
#endif 
        }

        public static void SetScrollViewHorizontalScrollVisible(this ScrollView scrollView, bool visible)
        {
#if UNITY_2021_2_OR_NEWER
            scrollView.horizontalScrollerVisibility = visible ? ScrollerVisibility.AlwaysVisible : ScrollerVisibility.Hidden;
#else
            scrollView.showHorizontal = visible;
#endif
        }
    }
}