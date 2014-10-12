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

namespace AvalonDock
{
    public class FloatingDockablePane : DockablePane
    {
        static FloatingDockablePane()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(FloatingDockablePane), new FrameworkPropertyMetadata(typeof(FloatingDockablePane)));            
            DockablePane.ShowHeaderProperty.OverrideMetadata(typeof(FloatingDockablePane), new FrameworkPropertyMetadata(false));
        }

        public FloatingDockablePane(FloatingWindow floatingWindow)
        {
            _floatingWindow = floatingWindow;
        }

        FloatingWindow _floatingWindow = null;

        public FloatingWindow FloatingWindow
        {
            get { return _floatingWindow; }
        }

        public override DockingManager GetManager()
        {
            return _floatingWindow.Manager;
        }

        protected override void CheckItems(System.Collections.IList newItems)
        {
            foreach (object newItem in newItems)
            {
                if (!(newItem is DockableContent) && !(newItem is DocumentContent))
                    throw new InvalidOperationException("FloatingDockablePane can contain only DockableContents and DocumentContents!");
            }
        }

    }
}
