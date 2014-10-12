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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using AvalonDock;
using System.IO;

namespace AvalonDockTest
{
    /// <summary>
    /// Interaction logic for Window5.xaml
    /// </summary>
    public partial class Window5 : Window
    {
        public Window5()
        {
            InitializeComponent();
        }

        MemoryStream savedLayout = new MemoryStream();

        private void ShowDockingManager_Click(object sender, RoutedEventArgs e)
        {
            savedLayout.Position = 0;
            _dockingManager.RestoreLayout(savedLayout);
            _dockingManager.Hide(ShowMeFirst);
            savedLayout.Close();
        }


        private void _dockingManager_Loaded(object sender, RoutedEventArgs e)
        {
            _dockingManager.SaveLayout(savedLayout);

            _dockingManager.Hide(ShowMeSecond);
            _dockingManager.Hide(AlsoShowMeSecond);

        }
    }
}
