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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Xml;
using System.Windows.Forms.Integration;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.Reflection;


namespace AvalonDock
{

    public abstract class ManagedContent : ContentControl, INotifyPropertyChanged
    {
        static ManagedContent()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(ManagedContent), new FrameworkPropertyMetadata(typeof(ManagedContent)));
            FocusableProperty.OverrideMetadata(typeof(ManagedContent), new FrameworkPropertyMetadata(true));
        }

        public ManagedContent()
        {
            this.Loaded += new RoutedEventHandler(ManagedContent_Loaded);
            this.Unloaded += new RoutedEventHandler(ManagedContent_Unloaded);
			this.LayoutUpdated += new EventHandler(ManagedContent_LayoutUpdated);
        }

		public void ManagedContent_LayoutUpdated(object sender, EventArgs e)
		{
            WindowsFormsHost contentHost = null;

			if (this.Content is UserControl)
			{
				UserControl usTemp = this.Content as UserControl;

                if (usTemp.Content is WindowsFormsHost)
                    contentHost = usTemp.Content as WindowsFormsHost;
			}
			else if (this.Content is WindowsFormsHost)
			{
                contentHost = this.Content as WindowsFormsHost;
			}

            if (contentHost != null)
            {
                object childCtrl = contentHost.GetType().GetProperty("Child").GetValue(contentHost, null);

                if (childCtrl != null)
                {
                    //childCtrl.CallMethod("Refresh", null);
                    this.Dispatcher.Invoke(new Action<object>((o) => o.CallMethod("Refresh", null)), DispatcherPriority.Render, childCtrl);
                }
            }
		}

        void ManagedContent_Loaded(object sender, RoutedEventArgs e)
        {
        }

        void ManagedContent_Unloaded(object sender, RoutedEventArgs e)
        {
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ManagedContent));

        //public string IconSource
        //{
        //    get { return (string)GetValue(IconSourceProperty); }
        //    set { SetValue(IconSourceProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty IconSourceProperty =
        //    DependencyProperty.Register("IconSource", typeof(string), typeof(ManagedContent));

        /// <summary>
        /// Access to <see cref="IconProperty"/> dependency property
        /// </summary>
        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// Select an icon object for the content
        /// </summary>
        public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register("Icon", typeof(object), typeof(ManagedContent),
        new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerce_Icon)));

        private static object OnCoerce_Icon(DependencyObject o, object value)
        {
            if (value is string)
            {
                Uri iconUri;
                // try to resolve given value as an absolute URI
                if (Uri.TryCreate(value as String, UriKind.RelativeOrAbsolute, out iconUri))
                {
                    ImageSource img = new BitmapImage(iconUri);
                    if (null != img)
                    {
                        GreyableImage icon = (o as ManagedContent).Icon as GreyableImage;
                        if (null == icon)
                            icon = new GreyableImage();

                        icon.Source = img;
                        icon.Stretch = Stretch.None;
                        icon.SnapsToDevicePixels = true;

                        return icon;
                    }
                }
            }
            return value;
        } 

        /// <summary>
        /// Access to <see cref="DefaultFocusedElementProperty"/>
        /// </summary>
        public IInputElement DefaultElement
        {

            get { return (IInputElement)GetValue(DefaultFocusedElementProperty); }

            set { SetValue(DefaultFocusedElementProperty, value); }

        }

        /// <summary>
        /// Gets or sets an element which is focused by default when content is activated
        /// </summary>
        public static readonly DependencyProperty DefaultFocusedElementProperty = DependencyProperty.Register("DefaultElement", typeof(IInputElement), typeof(ManagedContent));


        FrameworkElement _dragEnabledArea;

        protected FrameworkElement DragEnabledArea
        {
            get { return _dragEnabledArea; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dragEnabledArea = GetTemplateChild("PART_DragArea") as FrameworkElement;

            if (_dragEnabledArea != null)
            {
                _dragEnabledArea.MouseDown += new MouseButtonEventHandler(OnDragMouseDown);
                _dragEnabledArea.MouseMove += new MouseEventHandler(OnDragMouseMove);
                _dragEnabledArea.MouseUp += new MouseButtonEventHandler(OnDragMouseUp);
                _dragEnabledArea.MouseLeave += new MouseEventHandler(OnDragMouseLeave);
            }
        }


        #region Mouse management

        protected virtual void OnDragStart(Point ptMouse, Point ptrelativeMouse)
        {

        }

        Point ptStartDrag;
        bool isMouseDown = false;

        protected Point StartDragPoint
        {
            get { return ptStartDrag; }
        }

        protected bool IsMouseDown
        {
            get { return isMouseDown; }
        }

        protected void ResetIsMouseDownFlag()
        {
            isMouseDown = false;
        }

        protected virtual void OnDragMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.Handled && Manager != null)// && State != DockableContentState.AutoHide)
            {
                isMouseDown = true;
                ptStartDrag = e.GetPosition((IInputElement)System.Windows.Media.VisualTreeHelper.GetParent(this));
            }
        }

        protected virtual void OnDragMouseMove(object sender, MouseEventArgs e)
        {
        }

        protected virtual void OnDragMouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        Point ptRelativePosition;

        protected virtual void OnDragMouseLeave(object sender, MouseEventArgs e)
        {
            if (!e.Handled && IsMouseDown && Manager != null)
            {
                if (!IsMouseCaptured)
                {
                    Point ptMouseMove = e.GetPosition(this);
                    ManagedContent contentToSwap = null;
                    if (ContainerPane != null)
                    {
                        foreach (ManagedContent content in ContainerPane.Items)
                        {
                            if (content == this)
                                continue;

                            HitTestResult res = VisualTreeHelper.HitTest(content, e.GetPosition(content));
                            if (res != null)
                            {
                                contentToSwap = content;
                                break;
                            }
                        }
                    }



                    if (contentToSwap != null)
                    {
                        Pane containerPane = ContainerPane;
                        int myIndex = containerPane.Items.IndexOf(this);

                        ContainerPane.Items.RemoveAt(myIndex);

                        int otherIndex = containerPane.Items.IndexOf(contentToSwap);
                        containerPane.Items.RemoveAt(otherIndex);

                        containerPane.Items.Insert(otherIndex, this);

                        containerPane.Items.Insert(myIndex, contentToSwap);

                        containerPane.SelectedItem = this;

                        e.Handled = false;
                        return;
                    }
                    else if (Math.Abs(ptMouseMove.X - StartDragPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(ptMouseMove.Y - StartDragPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        ptRelativePosition = e.GetPosition(DragEnabledArea);

                        ResetIsMouseDownFlag();
                        OnDragStart(StartDragPoint, ptRelativePosition);
                        e.Handled = true;
                    }
                }
            }

            isMouseDown = false;
        }


        #endregion

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (!e.Handled)
            {
                SetAsActive();
                IInputElement focusedElement = e.Source as IInputElement;
                if (focusedElement != null) Keyboard.Focus(focusedElement);
            }

        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ContainerPane != null)
                    ContainerPane.SelectedItem = this;
            }

            base.OnKeyDown(e);
        }

        public Pane ContainerPane
        {
            get 
            {
                Pane containerPane = Parent as Pane;
                if (containerPane != null)
                    return containerPane;

                return this.FindVisualAncestor<Pane>(false);
            }
        }

        internal DockingManager Manager
        {
            get 
            {
                if (ContainerPane != null)
                    return ContainerPane.GetManager();

                return null;
            }
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            Debug.WriteLine(string.Format("[{0}].OnGotKeyboardFocus() Source={1} NewFocus={2} OldFocus={3}", this.Name, e.Source.GetType().ToString(), e.NewFocus.GetType().ToString(), e.OldFocus == null ? "<null>" : e.OldFocus.GetType().ToString()));

            if (Manager != null && this.IsKeyboardFocusWithin)// && Manager.ActiveContent != this)
            {
                Manager.ActiveContent = this;
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Debug.WriteLine(string.Format("[{0}].OnLostKeyboardFocus() Source={1} NewFocus={2} OldFocus={3}", this.Name, e.Source.GetType().ToString(), e.NewFocus == null ? "<null>" : e.NewFocus.GetType().ToString(), e.OldFocus == null ? "<null>" : e.OldFocus.GetType().ToString()));
            base.OnLostKeyboardFocus(e);
        }

        bool _isActiveContent = false;

        /// <summary>
        /// Returns true if the content is the currently active content.
        /// </summary>
        /// <remarks>Use <see cref="SetAsActive"/> method to set a content as active.</remarks>
        public bool IsActiveContent
        {
            get 
            {
                return _isActiveContent;  
            }
            internal set 
            {
                if (_isActiveContent != value)
                {
                    _isActiveContent = value;
                    NotifyPropertyChanged("IsActiveContent");
                    if (IsActiveContentChanged != null)
                        IsActiveContentChanged(this, EventArgs.Empty);

                    if (_isActiveContent && !IsKeyboardFocused)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(delegate
                        {
                            if (_isActiveContent && !IsKeyboardFocused)
                            {
                                if (this.Content is WindowsFormsHost)
                                {
                                    //Use reflection in order to remove WinForms assembly reference
                                    WindowsFormsHost contentHost = this.Content as WindowsFormsHost;

                                    object childCtrl = contentHost.GetType().GetProperty("Child").GetValue(contentHost, null);

                                    if (childCtrl != null)
                                    {
                                        if (!childCtrl.GetPropertyValue<bool>("Focused"))
                                        {
                                            childCtrl.CallMethod("Focus", null);
                                        }
                                    }
                                }
                                else if (DefaultElement != null)
                                {
                                    Debug.WriteLine("Try to set kb focus to " + DefaultElement);

                                    IInputElement kbFocused = Keyboard.Focus(DefaultElement);

                                    if (kbFocused != null)
                                        Debug.WriteLine("Focused element " + kbFocused);
                                    else
                                        Debug.WriteLine("No focused element");

                                }
                                else if (this.Content is IInputElement)
                                {
                                    //Debug.WriteLine("Try to set kb focus to " + this.Content.ToString());
                                    //IInputElement kbFocused = Keyboard.Focus(this.Content as IInputElement);
                                    //if (kbFocused != null)
                                    //    Debug.WriteLine("Focused element " + kbFocused);
                                    //else
                                    //    Debug.WriteLine("No focused element");
                                }
                            }
                        }));
                    }
                }

            }
        }

        public event EventHandler IsActiveContentChanged;

        /// <summary>
        /// Set the content as the active content
        /// </summary>
        /// <remarks>After this method returns property <see cref="IsActiveContent"/> returns true.</remarks>
        public void SetAsActive()
        {
            if (ContainerPane != null && Manager != null)// && Manager.ActiveContent != this)
            {
                ContainerPane.SelectedItem = this;
                ContainerPane.Focus();
                if (Manager != null)
                    Manager.ActiveContent = this;
            }
        }

        bool _isActiveDocument = false;

        /// <summary>
        /// Returns true if the document is the currently active document.
        /// </summary>
        /// <remarks>Use <see cref="SetAsActive"/> method to set a content as active.</remarks>
        public bool IsActiveDocument
        {
            get
            {
                return _isActiveDocument;
            }
            internal set
            {
                if (_isActiveDocument != value)
                {
                    if (value)
                    {
                        if (ContainerPane != null)
                            ContainerPane.SelectedItem = this;
                    }

                    _isActiveDocument = value;
                    NotifyPropertyChanged("IsActiveDocument");
                }
            }
        }

        bool _isLocked;

        /// <summary>
        /// Gets or sets a value indicating if this content is locked (readonly).
        /// </summary>
        public bool IsLocked
        {
            get { return _isLocked; }
            set
            {
                _isLocked = value;
                NotifyPropertyChanged("IsLocked");
            }
        }

        Size _floatingWindowSize = Size.Empty;

        /// <summary>
        /// Gets or sets the size of the floating window which hosts this content
        /// </summary>
        public Size FloatingWindowSize
        {
            get
            { return _floatingWindowSize; }
            set
            { _floatingWindowSize = value; }
        }


        ResizeMode _floatingResizeMode = ResizeMode.CanResize;
        public ResizeMode FloatingResizeMode
        {
            get
            { return _floatingResizeMode; }
            set
            { _floatingResizeMode = value; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public bool IsCloseable
        {
            get { return (bool)GetValue(IsCloseableProperty); }
            set { SetValue(IsCloseableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCloseable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCloseableProperty =
            DependencyProperty.Register("IsCloseable", typeof(bool), typeof(ManagedContent), new UIPropertyMetadata(true));


    }
}
