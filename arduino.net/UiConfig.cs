using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace arduino.net
{
    public class UiConfig
    {
        public const string Color0 = "C0";
        public const string Color1 = "C1";
        public const string Color5 = "C5";
        public const string Color6 = "C6";
        public const string TextOnColor0 = "C0-Text";
        public const string TextOnBackground0 = "B0-Text";


        public const string Background0 = "B0";
        public const string Background1 = "B1";


        public static System.Drawing.Color GetWinformsColor(string resourceName)
        {
            var b = Application.Current.Resources[resourceName] as System.Windows.Media.SolidColorBrush;
            if (b == null) return System.Drawing.Color.Red;

            var c = b.Color;
            return System.Drawing.Color.FromArgb((int)(c.R), (int)(c.G), (int)(c.B));
        }

        public static System.Windows.Media.SolidColorBrush GetBrush(string resourceName)
        {
            return Application.Current.Resources[resourceName] as System.Windows.Media.SolidColorBrush;
        }
    }
}
