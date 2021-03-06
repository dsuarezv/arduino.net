﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CapturePointsPad.xaml
    /// </summary>
    public partial class CapturePointsPad : UserControl
    {
        public CapturePointsPad()
        {
            InitializeComponent();

            CaptureMonitorFactory.RegisterCaptureMonitorControl(typeof(BasicCaptureMonitor));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            MainListBox.ItemsSource = IdeManager.CapturePointManager.CapturePoints;
        }
    }
}
