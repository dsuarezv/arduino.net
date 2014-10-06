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
    /// <summary>
    /// Defines an interface that must be implemented by objects that can host dragged panes
    /// </summary>
    internal interface IDropSurface
    {
        /// <summary>
        /// Gets a value indicating if this area is avilable for drop a dockable pane
        /// </summary>
        bool IsSurfaceVisible { get; }

        /// <summary>
        /// Gets the sensible area for drop a pane
        /// </summary>
        Rect SurfaceRectangle { get; }

        /// <summary>
        /// Called by <see cref="DragPaneService"/> when user dragged pane enter this surface
        /// </summary>
        /// <param name="point">Location of the mouse</param>
        void OnDragEnter(Point point);

        /// <summary>
        /// Called by <see cref="DragPaneService"/> when user dragged pane is over this surface
        /// </summary>
        /// <param name="point">Location of the mouse</param>
        void OnDragOver(Point point);

        /// <summary>
        /// Called by <see cref="DragPaneService"/> when user dragged pane leave this surface
        /// </summary>
        /// <param name="point">Location of the mouse</param>
        void OnDragLeave(Point point);

        /// <summary>
        /// Called by <see cref="DragPaneService"/> when user drops a pane to this surface
        /// </summary>
        /// <param name="point">Location of the mouse</param>
        bool OnDrop(Point point);
    }
}
