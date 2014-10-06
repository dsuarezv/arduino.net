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
using System.Linq;
using System.Windows.Markup;

namespace AvalonDock
{
    public abstract class PaneTabPanel : Panel
    {
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            ManagedContent mc = visualAdded as ManagedContent;
            if (mc != null)
            {
                mc.Style = TabItemStyle;
                mc.ApplyTemplate();
            }

        }


        internal PaneTabPanel()
        { 

        }

        #region TabItemStyle

        /// <summary>
        /// TabItemStyle Dependency Property
        /// </summary>
        public static readonly DependencyProperty TabItemStyleProperty =
            DependencyProperty.Register("TabItemStyle", typeof(Style), typeof(PaneTabPanel),
                new FrameworkPropertyMetadata((Style)null,
                    new PropertyChangedCallback(OnTabItemStyleChanged)));

        /// <summary>
        /// Gets or sets the TabItemStyle property.  This dependency property 
        /// indicates style to use for tabs.
        /// </summary>
        public Style TabItemStyle
        {
            get { return (Style)GetValue(TabItemStyleProperty); }
            set { SetValue(TabItemStyleProperty, value); }
        }

        /// <summary>
        /// Handles changes to the TabItemStyle property.
        /// </summary>
        private static void OnTabItemStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PaneTabPanel)d).OnTabItemStyleChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TabItemStyle property.
        /// </summary>
        protected virtual void OnTabItemStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            //Children.Cast<ManagedContent>().ForEach(c =>
            //    {
            //        Binding bnd = new Binding("TabItemStyle");
            //        bnd.Source = this;
            //        bnd.Mode = BindingMode.OneWay;

            //        c.SetBinding(StyleProperty, bnd);

            //        //c.Style = TabItemStyle;
            //    });
        }


        #endregion
    }
}
