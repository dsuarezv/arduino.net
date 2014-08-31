﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace arduino.net
{
    /// <summary>
    /// Interaction logic for SystemButtons.xaml
    /// </summary>
    public partial class SystemButtons : UserControl
    {
        public SystemButtons()
        {
            InitializeComponent();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var w = this.Parent as Window;

            if (w == null) return;

            w.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
