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

namespace AvalonDock
{
    public class ResizingPanelSplitter : System.Windows.Controls.Primitives.Thumb
    {
        static ResizingPanelSplitter()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizingPanelSplitter), new FrameworkPropertyMetadata(typeof(ResizingPanelSplitter)));
            MinWidthProperty.OverrideMetadata(typeof(ResizingPanelSplitter), new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
            MinHeightProperty.OverrideMetadata(typeof(ResizingPanelSplitter), new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
            HorizontalAlignmentProperty.OverrideMetadata(typeof(ResizingPanelSplitter), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
            VerticalAlignmentProperty.OverrideMetadata(typeof(ResizingPanelSplitter), new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            protected set { SetValue(OrientationPropertyKey, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        private static readonly DependencyPropertyKey OrientationPropertyKey =
            DependencyProperty.RegisterReadOnly("Orientation", typeof(Orientation), typeof(ResizingPanelSplitter), new UIPropertyMetadata(Orientation.Horizontal));

        public static readonly DependencyProperty OrientationProperty = OrientationPropertyKey.DependencyProperty;

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            ResizingPanel panel = Parent as ResizingPanel;
            if (panel != null)
                Orientation = panel.Orientation;

            base.OnVisualParentChanged(oldParent);
        }
    }
}
