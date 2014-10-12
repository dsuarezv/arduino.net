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

namespace AvalonDock
{
    /// <summary>
    /// Defines a list of brushes used by AvalonDock templates
    /// </summary>
    public enum AvalonDockBrushes
    {
        /// <summary>
        /// Default brush for DockingManager background
        /// </summary>
        DefaultBackgroundBrush,

        /// <summary>
        /// Brush used for the title background of a <see cref="DockablePane"/>.
        /// </summary>
        DockablePaneTitleBackground,

        /// <summary>
        /// Brush used for the title background of a <see cref="DockablePane"/> when is focused.
        /// </summary>
        DockablePaneTitleBackgroundSelected,

        /// <summary>
        /// Brush used for the title foreground of a <see cref="DockablePane"/>.
        /// </summary>
        DockablePaneTitleForeground,

        /// <summary>
        /// Brush used for the title foreground of a <see cref="DockablePane"/> when is focused.
        /// </summary>
        DockablePaneTitleForegroundSelected,

        /// <summary>
        /// Brush used for the background of the pane command pins.
        /// </summary>
        PaneHeaderCommandBackground,

        /// <summary>
        /// Brush used for the border of the pane command pins.
        /// </summary>
        PaneHeaderCommandBorderBrush,

        /// <summary>
        /// Brush used for the background of a document header.
        /// </summary>
        DocumentHeaderBackground,

        /// <summary>
        /// Brush used for the foreground of a document header.
        /// </summary>
        DocumentHeaderForeground,

        /// <summary>
        /// Brush used for fonts while a document header is selected but not activated
        /// </summary>
        DocumentHeaderForegroundSelected,

        /// <summary>
        /// Brush used for fonts while a document header is selected and activated
        /// </summary>
        DocumentHeaderForegroundSelectedActivated,

        /// <summary>
        /// Brush used for the background of a document header when selected (<see cref="ManagedContent.IsActiveContent"/>).
        /// </summary>
        DocumentHeaderBackgroundSelected,

        /// <summary>
        /// Brush used for the background of a document header when active and selected (<see cref="ManagedContent.IsActiveDocument"/>).
        /// </summary>
        DocumentHeaderBackgroundSelectedActivated,

        /// <summary>
        /// Brush used for the background of a document header when mouse is over it.
        /// </summary>
        DocumentHeaderBackgroundMouseOver,

        /// <summary>
        /// Brush used for the border brush of a document header when mouse is over it (but is not selected).
        /// </summary>
        DocumentHeaderBorderBrushMouseOver,

        /// <summary>
        /// Brush for the document header border
        /// </summary>
        DocumentHeaderBorder,

        /// <summary>
        /// Brush for the document header border when contains a document selected
        /// </summary>
        DocumentHeaderBorderSelected,

        /// <summary>
        /// Brush for the document header border when contains a document selected and activated
        /// </summary>
        DocumentHeaderBorderSelectedActivated,



        NavigatorWindowTopBackground,

        NavigatorWindowTitleForeground,

        NavigatorWindowDocumentTypeForeground,

        NavigatorWindowInfoTipForeground,

        NavigatorWindowForeground,

        NavigatorWindowBackground,

        NavigatorWindowSelectionBackground,

        NavigatorWindowSelectionBorderbrush,

        NavigatorWindowBottomBackground,
    }
}
