/************************************************************************

   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the New BSD
   License (BSD) as published at http://avalondock.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up AvalonDock in Extended WPF Toolkit Plus at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like facebook.com/datagrids

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AvalonDock
{
    //public class FindResourcePathConverter : IValueConverter
    //{
    //    #region IValueConverter Members

    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (value == null)
    //        {
    //            return null;
    //            //return new Uri(@"DocumentHS.png", UriKind.Relative);
    //        }

    //        return value;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    #endregion
    //}

    /// <summary>
    /// Converter from boolean values to visibility (inverse mode)
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToBoolean(value) ? Visibility.Visible :
            (parameter != null && ((string)parameter) == "Hidden" ? Visibility.Hidden : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(object), typeof(Image))]
    public class ObjectToImageConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double width = 16.0;
            if (parameter != null &&
                parameter is double)
                width = (double)parameter;

            if (value is string)
            {
                Uri iconUri;
                // try to resolve given value as an absolute URI
                if (Uri.TryCreate(value as String, UriKind.RelativeOrAbsolute, out iconUri))
                {
                    var img = new BitmapImage(iconUri);
                    if (img != null)
                    {
                        return new Image()
                        {
#if NET4
                                UseLayoutRounding = true,
#endif
                            Width = width,
                            Source = img
                        };
                    }
                }
            }
            else if (value is BitmapImage)
            {
                var img = value as BitmapImage;
                return new Image()
                {
#if NET4
                    UseLayoutRounding = true,
#endif
                    Width = width,
                    Source = new BitmapImage(img.UriSource)
                };
            }


            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public static class Converters
    {
        static BoolToVisibilityConverter _BoolToVisibilityConverter = null;

        public static BoolToVisibilityConverter BoolToVisibilityConverter
        {
            get
            {
                if (_BoolToVisibilityConverter == null)
                    _BoolToVisibilityConverter = new BoolToVisibilityConverter();


                return _BoolToVisibilityConverter;
            }
        }

        static ObjectToImageConverter _ObjectToImageConverter = null;

        public static ObjectToImageConverter ObjectToImageConverter
        {
            get
            {
                if (_ObjectToImageConverter == null)
                    _ObjectToImageConverter = new ObjectToImageConverter();


                return _ObjectToImageConverter;
            }
        }

    }
}
