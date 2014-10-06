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
using System.Diagnostics;

namespace AvalonDockTest
{
    /// <summary>
    /// Interaction logic for DemoDocument.xaml
    /// </summary>
    public partial class DemoDocument : DocumentContent
    {
        public DemoDocument()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }


        private void OnCanClose(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.Handled = true;
            //e.CanExecute = false;
        }

        public bool IsChanged { get; set; }

        public static readonly RoutedCommand ViewCommand = new RoutedCommand();
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void DocumentContent_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(this.Title + " loaded");
        }

        protected override void OnInitialized(EventArgs e)
        {
            Debug.WriteLine(this.Title + " initialized");
            base.OnInitialized(e);
        }



        private void bntClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
