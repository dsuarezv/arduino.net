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

namespace AvalonDock
{
    class OverlayWindowDockingButton : IDropSurface
    {
        OverlayWindow _owner;
        FrameworkElement _btnDock;

        public OverlayWindowDockingButton(FrameworkElement btnDock, OverlayWindow owner)
            : this(btnDock, owner, true)
        {

        }
        public OverlayWindowDockingButton(FrameworkElement btnDock, OverlayWindow owner, bool enabled)
        {
            if (btnDock == null)
                throw new ArgumentNullException("btnDock");
            if (owner == null)
                throw new ArgumentNullException("owner");

            _btnDock = btnDock;
            _owner = owner;
            Enabled = enabled;
        }

        bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set 
            {
                _enabled = value;

                if (_enabled)
                    _btnDock.Visibility = Visibility.Visible;
                else
                    _btnDock.Visibility = Visibility.Hidden;
            }
        }



        #region IDropSurface Membri di



        Rect IDropSurface.SurfaceRectangle
        {
            get
            {
                if (!(this as IDropSurface).IsSurfaceVisible)
                    return Rect.Empty;

                if (PresentationSource.FromVisual(_btnDock) == null)
                    return Rect.Empty;

                return new Rect(HelperFunc.PointToScreenWithoutFlowDirection(_btnDock, new Point()), new Size(_btnDock.ActualWidth, _btnDock.ActualHeight));
            }
        }

        void IDropSurface.OnDragEnter(Point point)
        {
            if (!Enabled)
                return;

            _owner.OnDragEnter(this, point);
        }

        void IDropSurface.OnDragOver(Point point)
        {
            if (!Enabled)
                return;

            _owner.OnDragOver(this, point);
        }

        void IDropSurface.OnDragLeave(Point point)
        {
            if (!Enabled)
                return;

            _owner.OnDragLeave(this, point);
        }

        bool IDropSurface.OnDrop(Point point)
        {
            if (!Enabled)
                return false;

            return _owner.OnDrop(this, point);
        }

        bool IDropSurface.IsSurfaceVisible
        {
            get { return (_owner.IsLoaded && _btnDock != null); }
        }

        #endregion
    }
}
