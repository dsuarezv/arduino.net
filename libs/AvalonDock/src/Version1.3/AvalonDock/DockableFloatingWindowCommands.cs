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
    /// Encapsulates the 
    /// </summary>
    public sealed class DockableFloatingWindowCommands
    {
        private static object syncRoot = new object();

        private static RoutedUICommand dockableCommand = null;
        public static RoutedUICommand SetAsDockableWindow
        {
            get
            {
                lock (syncRoot)
                {
                    if (null == dockableCommand)
                    {
                        dockableCommand = new RoutedUICommand(AvalonDock.Properties.Resources.DockableContentCommands_DockableFloatingWindow, "DockableWindow", typeof(DockableFloatingWindowCommands));
                    }
                }
                return dockableCommand;
            }
        }

        private static RoutedUICommand floatingCommand = null;
        public static RoutedUICommand SetAsFloatingWindow
        {
            get
            {
                lock (syncRoot)
                {
                    if (null == floatingCommand)
                    {
                        floatingCommand = new RoutedUICommand(AvalonDock.Properties.Resources.DockableContentCommands_FloatingWindow, "FloatingWindow", typeof(DockableFloatingWindowCommands));
                    }
                }
                return floatingCommand;
            }
        }

        //private static RoutedUICommand closeCommand = null;
        //public static RoutedUICommand Close
        //{
        //    get
        //    {
        //        lock (syncRoot)
        //        {
        //            if (null == closeCommand)
        //            {
        //                closeCommand = new RoutedUICommand("Close", "Close", typeof(FloatingWindowCommands));
        //            }
        //        }
        //        return closeCommand;
        //    }
        //}

        //private static RoutedUICommand dockCommand = null;
        //public static RoutedUICommand Dock
        //{
        //    get
        //    {
        //        lock (syncRoot)
        //        {
        //            if (null == dockCommand)
        //            {
        //                dockCommand = new RoutedUICommand("Dock", "Dock", typeof(FloatingWindowCommands));
        //            }
        //        }
        //        return dockCommand;
        //    }
        //}
    }
}
