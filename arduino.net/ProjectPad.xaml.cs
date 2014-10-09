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
    /// Interaction logic for ProjectPad.xaml
    /// </summary>
    public partial class ProjectPad : UserControl
    {
        public TabControl TargetTabControl
        {
            get { return MainTabSelector.TargetTabControl; }
            set { MainTabSelector.TargetTabControl = value; }
        }

        public ProjectPad()
        {
            InitializeComponent();
        }


    }
}