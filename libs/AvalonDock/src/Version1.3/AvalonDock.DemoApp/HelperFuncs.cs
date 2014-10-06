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
using System.IO;

namespace AvalonDock.DemoApp
{
    public static class HelperFuncs
    {
        public static string LayoutToString(this DockingManager dockManager)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            dockManager.SaveLayout(sw);
            return sb.ToString();
        }

        public static void LayoutFromString(this DockingManager dockManager, string layoutXml)
        {
            StringReader sr = new StringReader(layoutXml);
            dockManager.RestoreLayout(sr);
        }

    }
}
