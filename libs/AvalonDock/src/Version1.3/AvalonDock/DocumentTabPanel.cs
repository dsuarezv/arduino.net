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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;

namespace AvalonDock
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DocumentTabPanel : PaneTabPanel
    {
        public static bool GetIsHeaderVisible(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHeaderVisibleProperty);
        }

        public static void SetIsHeaderVisible(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHeaderVisibleProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsHeaderVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHeaderVisibleProperty =
            DependencyProperty.RegisterAttached("IsHeaderVisible", typeof(bool), typeof(DocumentTabPanel), new UIPropertyMetadata(false));

        protected override Size MeasureOverride(Size availableSize)
        {
            Size desideredSize = new Size();
            int i = 1;

            foreach (ManagedContent child in Children)
            {
                Panel.SetZIndex(child, Selector.GetIsSelected(child) ? 1 : -i);
                i++;
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                desideredSize.Width += child.DesiredSize.Width;
                desideredSize.Height = Math.Max(desideredSize.Height, child.DesiredSize.Height);
            }

            return new Size(Math.Min(desideredSize.Width, availableSize.Width), desideredSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double offset = 0.0;
            bool skipAllOthers = false;
            foreach (ManagedContent doc in Children)
            {
                if (skipAllOthers || offset + doc.DesiredSize.Width > finalSize.Width)
                {
                    SetIsHeaderVisible(doc, false);
                    doc.Arrange(new Rect());
                    skipAllOthers = true;
                }
                else
                {
                    SetIsHeaderVisible(doc, true);
                    doc.Arrange(new Rect(offset, 0.0, doc.DesiredSize.Width, finalSize.Height));
                    offset += doc.ActualWidth;
                }
            }

            return finalSize;

        }

    }
}
