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

namespace AvalonDock
{
    /// <summary>
    /// Defines a list of context menu resources
    /// </summary>
    public enum ContextMenuElement
    {
        /// <summary>
        /// Context menu related to a <see cref="DockablePane"/>
        /// </summary>
        DockablePane,

        /// <summary>
        /// Context menu related to a <see cref="DocumentPane"/>
        /// </summary>
        DocumentPane,

        /// <summary>
        /// Context menu related to a <see cref="DockableFloatingWindow"/>
        /// </summary>
        DockableFloatingWindow,

        /// <summary>
        /// Context menu related to a <see cref="DocumentFloatingWindow"/>
        /// </summary>
        DocumentFloatingWindow

    }
}
