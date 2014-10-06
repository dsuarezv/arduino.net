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
    public sealed class PaneCommands
    {
        static object syncRoot = new object();




        private static RoutedUICommand dockCommand = null;

        /// <summary>
        /// Dock <see cref="Pane"/> to container <see cref="DockingManager"/>
        /// </summary>
        public static RoutedUICommand Dock
        {
            get
            {
                lock (syncRoot)
                {
                    if (null == dockCommand)
                    {
                        dockCommand = new RoutedUICommand("Dock", "Dock", typeof(PaneCommands));
                    }
                }
                return dockCommand;
            }
        }

    }
}
