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
using System.Windows.Input;

namespace AvalonDock
{
    internal class FlyoutDockablePane : DockablePane
    {
        static FlyoutDockablePane()
        {
            DockablePane.ShowTabsProperty.AddOwner(typeof(FlyoutDockablePane), new FrameworkPropertyMetadata(false));
        }

        int _arrayIndexPreviousPane = -1;


        public FlyoutDockablePane()
        { }

        public FlyoutDockablePane(DockableContent content)
        {
            _referencedPane = content.ContainerPane as DockablePane;
            _manager = _referencedPane.GetManager();

            //save current content position in container pane
            _arrayIndexPreviousPane = _referencedPane.Items.IndexOf(content);
            Anchor = _referencedPane.Anchor;

            //SetValue(ResizingPanel.ResizeWidthProperty, new GridLength(ResizingPanel.GetEffectiveSize(_referencedPane).Width));
            //SetValue(ResizingPanel.ResizeHeightProperty, new GridLength(ResizingPanel.GetEffectiveSize(_referencedPane).Height));

            this.Style = _referencedPane.Style;

            //remove content from container pane
            //and add content to my temporary pane
            _referencedPane.Items.RemoveAt(_arrayIndexPreviousPane);
            this.Items.Add(content);


            //select the single content in this pane
            SelectedItem = this.Items[0];
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        internal void RestoreOriginalPane()
        {
            if (this.Items.Count == 1)
            {
                _referencedPane.Items.Insert(_arrayIndexPreviousPane, RemoveContent(0));
                //ResizingPanel.SetResizeWidth(_referencedPane, ResizingPanel.GetResizeWidth(this));
                //ResizingPanel.SetResizeHeight(_referencedPane, ResizingPanel.GetResizeHeight(this));
            }            
        }


        DockablePane _referencedPane = null;

        internal DockablePane ReferencedPane
        {
            get { return _referencedPane; }
        }

        DockingManager _manager = null;

        public override DockingManager GetManager()
        {
            return _manager;
        }

        public override void ToggleAutoHide()
        {
            GetManager().ToggleAutoHide(_referencedPane);
        }
   }
}
