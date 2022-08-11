//  Copyright (c) 2020-present amlovey
//  
using UnityEngine;

namespace APlus2
{
    public static class ColorHelper
    {
        public static Color Parse(string colorString)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString(colorString, out color))
            {
                return color;
            }

            throw new System.ArgumentException("color string is incorrect!");
        }

        public static string ToColorString(this Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
    }
}