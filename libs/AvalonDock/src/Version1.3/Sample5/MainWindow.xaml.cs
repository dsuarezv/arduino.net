﻿/************************************************************************

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AvalonDock;

namespace Sample5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Tools_Click(object sender, RoutedEventArgs e)
        {
            if (dcTools.State != DockableContentState.Docked)
                dcTools.Show(dockManager, AnchorStyle.Top);
            dcTools.Activate();
        }

        private void ProjectExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (dcProjectExplore.State != DockableContentState.Docked)
                dcProjectExplore.Show(dockManager, AnchorStyle.Top);
            dcProjectExplore.Activate();

        }

        private void PropertiesWindow_Click(object sender, RoutedEventArgs e)
        {
            if (dcPropertiesWindow.State != DockableContentState.Docked)
                dcPropertiesWindow.Show(dockManager, AnchorStyle.Bottom);
            dcPropertiesWindow.Activate();

        }
    }
}
