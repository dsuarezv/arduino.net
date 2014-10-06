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
using System.ComponentModel;
using System.Diagnostics;

namespace AvalonDock
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DockablePaneAnchorTab : System.Windows.Controls.Control, INotifyPropertyChanged
    {
        static DockablePaneAnchorTab()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockablePaneAnchorTab), new FrameworkPropertyMetadata(typeof(DockablePaneAnchorTab)));
        }

        public DockableContent ReferencedContent
        {
            get { return (DockableContent)GetValue(ReferencedContentPropertyKey.DependencyProperty); }
            set { SetValue(ReferencedContentPropertyKey, value); }
        }

        // Using a DependencyProperty as the backing store for DockableContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyPropertyKey ReferencedContentPropertyKey =
            DependencyProperty.RegisterReadOnly("ReferencedContent", typeof(DockableContent), typeof(DockablePaneAnchorTab), new UIPropertyMetadata(null, new PropertyChangedCallback(OnPaneAttached)));


        static void OnPaneAttached(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ReferencedContentPropertyKey.DependencyProperty)
            {
                DockablePaneAnchorTab _this = depObj as DockablePaneAnchorTab;
                if (_this.PropertyChanged != null)
                {
                    _this.PropertyChanged(depObj, new PropertyChangedEventArgs("Anchor"));
                    _this.PropertyChanged(depObj, new PropertyChangedEventArgs("Icon"));
                    _this.PropertyChanged(depObj, new PropertyChangedEventArgs("ReferencedContent"));
                }
            }

        }

        public AnchorStyle Anchor
        {
            get { return (AnchorStyle)GetValue(AnchorPropertyKey.DependencyProperty); }
            internal set { SetValue(AnchorPropertyKey, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyPropertyKey AnchorPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("Anchor", typeof(AnchorStyle), typeof(DockablePaneAnchorTab), new PropertyMetadata(AnchorStyle.Left));

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (ReferencedContent != null)
                ReferencedContent.Manager.ShowFlyoutWindow(ReferencedContent);

            base.OnMouseMove(e);
        }



        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(DockablePaneAnchorTab));

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


    }
}
