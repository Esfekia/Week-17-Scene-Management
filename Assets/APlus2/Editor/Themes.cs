//  Copyright (c) 2020-present amlovey
//  
using System;

namespace APlus2
{
    public enum ThemeType
    {
        Personal,
        Professional,
    }

    public class Theme
    {
        public string BackgroundColor;
        public string ForegroundColor;
        public string StatusBarBackgroundColor;
        public string DialogBackgroundColor;
    }
    
    public class Themes
    {
        private static Theme theme;
        public static Theme Current
        {
            get
            {
                if (theme == null)
                {
                    Init();
                }

                return theme;
            }
        }

        private static Theme Personal = new Theme()
        {
            BackgroundColor = "#c2c2c2",
            ForegroundColor = "#202020",
            StatusBarBackgroundColor = "#e0e0e0",
            DialogBackgroundColor = "#e0e0e0",
        };

        private static Theme Professional = new Theme()
        {
            BackgroundColor = "#383838",
            ForegroundColor = "#bfbfbf",
            StatusBarBackgroundColor = "#4f4f4f",
            DialogBackgroundColor = "#404040"
        };

        public static void ChangeTheme(ThemeType type)
        {
            switch (type)
            {
                case ThemeType.Personal:
                    theme = Personal;
                    Settings.Theme = ThemeType.Personal.ToString();
                    break;
                case ThemeType.Professional:
                    theme = Professional;
                    Settings.Theme = ThemeType.Professional.ToString();
                    break;
            }
        }

        public static void Init()
        {
            ThemeType t;
            if (Enum.TryParse<ThemeType>(Settings.Theme, true, out t))
            {
                ChangeTheme(t);
            }
        }
    }
}
