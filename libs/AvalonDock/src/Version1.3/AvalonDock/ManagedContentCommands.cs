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
    /// Defines commands shared beteween all contents (Dockable or Documents)
    /// </summary>
    public sealed class ManagedContentCommands
    {
        private static object syncRoot = new object();


        private static RoutedUICommand closeCommand = null;

        /// <summary>
        /// This command closes the content
        /// </summary>
        public static RoutedUICommand Close
        {
            get
            {
                lock (syncRoot)
                {
                    if (null == closeCommand)
                    {
                        closeCommand = new RoutedUICommand(AvalonDock.Properties.Resources.ManagedContentCommands_Close, "Close", typeof(ManagedContentCommands));
                    }
                }
                return closeCommand;
            }
        }

        private static RoutedUICommand hideCommand = null;

        /// <summary>
        /// This command hides the content
        /// </summary>
        public static RoutedUICommand Hide
        {
            get
            {
                lock (syncRoot)
                {
                    if (null == hideCommand)
                    {
                        hideCommand = new RoutedUICommand(AvalonDock.Properties.Resources.ManagedContentCommands_Hide, "Hide", typeof(ManagedContentCommands));
                    }
                }
                return hideCommand;
            }
        }

        private static RoutedUICommand showCommand = null;

        /// <summary>
        /// This command shows the content
        /// </summary>
        /// <remarks>How content is shown by default depends from the type of content. A <see cref="DockableContent"/> is shown as docked pane, instead
        /// a <see cref="DocumentContent"/> is shown as tabbed document</remarks>
        public static RoutedUICommand Show
        {
            get
            {
                lock (syncRoot)
                {
                    if (null == showCommand)
                    {
                        showCommand = new RoutedUICommand(AvalonDock.Properties.Resources.ManagedContentCommands_Show, "Show", typeof(ManagedContentCommands));
                    }
                }
                return showCommand;
            }
        }


        private static RoutedUICommand activateCommand = null;

        /// <summary>
        /// This command activate the commands (i.e. select it inside the conatiner pane)
        /// </summary>
        /// <remarks>Activating a content means essentially putting it in evidence. For a content that is auto-hidden this command opens a flyout window containing the content.</remarks>
        public static RoutedUICommand Activate
        {
            get
            {
                lock (syncRoot)
                {
                    if (null == activateCommand)
                    {
                        activateCommand = new RoutedUICommand(AvalonDock.Properties.Resources.ManagedContentCommands_Activate, "Activate", typeof(ManagedContentCommands));
                    }
                }
                return activateCommand;
            }
        }
    }
}
