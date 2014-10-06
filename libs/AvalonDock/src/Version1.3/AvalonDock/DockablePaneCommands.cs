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
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace AvalonDock
{
    /// <summary>
    /// Defines commands that can be applied to a dockable pane
    /// </summary>
    public sealed class DockablePaneCommands
    {
        private static object syncRoot = new object();

        private static RoutedUICommand closeCommand = null;

        /// <summary>
        /// This command closes the <see cref="DockablePane"/> and closes all the contained <see cref="DockableContent"/>s inside it
        /// </summary>
        public static RoutedUICommand Close
        {
            get
            {
                lock (syncRoot)
                {
                    if (null == closeCommand)
                    {
                        closeCommand = new RoutedUICommand(AvalonDock.Properties.Resources.DockablePaneCommands_Close, "Close", typeof(DockablePaneCommands));
                    }
                }
                return closeCommand;
            }
        }

        private static RoutedUICommand hideCommand = null;

        /// <summary>
        /// This command closes the <see cref="DockablePane"/> and hides all the contained <see cref="DockableContent"/>s inside it
        /// </summary>
        public static RoutedUICommand Hide
        {
            get
            {
                lock (syncRoot)
                {
                    if (null == hideCommand)
                    {
                        hideCommand = new RoutedUICommand(AvalonDock.Properties.Resources.DockablePaneCommands_Hide, "Hide", typeof(DockablePaneCommands));
                    }
                }
                return hideCommand;
            }
        }

        private static RoutedUICommand autoHideCommand = null;

        /// <summary>
        /// This commands auto-hides the pane with all contained <see cref="DockableContent"/>s inside it
        /// </summary>
        public static RoutedUICommand ToggleAutoHide
        {
            get
            {
                lock (syncRoot)
                {
                    if (null == autoHideCommand)
                    {
                        autoHideCommand = new RoutedUICommand(AvalonDock.Properties.Resources.DockablePaneCommands_ToggleAutoHide, "AutoHide", typeof(DockablePaneCommands));
                    }
                }
                return autoHideCommand;
            }
        }

    }
}
