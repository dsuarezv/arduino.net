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
using System.Windows;
using System.Data;
using System.Xml;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Controls;

namespace AvalonDockTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : System.Windows.Application
    {

        private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            PresentationTraceSources.ResourceDictionarySource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.ResourceDictionarySource.Switch.Level = SourceLevels.All;
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            PresentationTraceSources.DependencyPropertySource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DependencyPropertySource.Switch.Level = SourceLevels.All;
            PresentationTraceSources.DocumentsSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DocumentsSource.Switch.Level = SourceLevels.All;
            PresentationTraceSources.MarkupSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.MarkupSource.Switch.Level = SourceLevels.All;
            PresentationTraceSources.NameScopeSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.NameScopeSource.Switch.Level = SourceLevels.All;
        }



    }
}
