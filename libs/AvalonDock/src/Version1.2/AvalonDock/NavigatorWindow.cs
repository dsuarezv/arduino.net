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
using System.ComponentModel;

namespace AvalonDock
{

    public class NavigatorWindowItem
    {
        private string _title;

        public string Title
        {
            get
            {
                return _title;
            }
        }

        private object _icon;

        public object Icon
        {
            get
            {
                return _icon;
            }
        }

        protected ManagedContent _content;

        public ManagedContent ItemContent
        {
            get { return _content; }
        }

        internal NavigatorWindowItem(ManagedContent content)
        {
            _title = content.Title;
            _icon = content.Icon;
            _content = content;
        }
    }

    public class NavigatorWindowDocumentItem : NavigatorWindowItem
    {
        private string _infoTip;

        public string InfoTip
        {
            get
            {
                return _infoTip;
            }
        }

        private string _contentTypeDescription;

        public string ContentTypeDescription
        {
            get
            {
                return _contentTypeDescription;
            }
        }

        private DateTime _lastActivation;

        public DateTime LastActivation
        {
            get { return _lastActivation; }
        }


        internal NavigatorWindowDocumentItem(DocumentContent document)
            : base(document)
        {
            _infoTip = document.InfoTip;
            if (_infoTip == null && document.ToolTip != null && document.ToolTip is string)
                _infoTip = document.ToolTip.ToString();

            _contentTypeDescription = document.ContentTypeDescription;
            _lastActivation = document.LastActivation;
        }


    }

    public class NavigatorWindow : Window, INotifyPropertyChanged
    {
        static NavigatorWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(typeof(NavigatorWindow)));


            Window.AllowsTransparencyProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(true));
            Window.WindowStyleProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(WindowStyle.None));
            Window.ShowInTaskbarProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(false));
            Control.BackgroundProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(Brushes.Transparent));
        }

        public NavigatorWindow()
        {
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Tab)
                CloseThisWindow();//Hide();
            else
            {
                e.Handled = true;
                MoveNextSelectedContent();
            }
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Tab)
                CloseThisWindow();//Hide();
            else
            {
                e.Handled = true;
            }
        }

        DockingManager _manager;
        public NavigatorWindow(DockingManager manager)
        {
            _manager = manager;
            Keyboard.AddKeyUpHandler(this, new KeyEventHandler(this.OnKeyUp));
            Keyboard.AddKeyDownHandler(this, new KeyEventHandler(this.OnKeyDown));
        }


        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            List<DocumentContent> listOfDocuments = _manager.FindContents<DocumentContent>();
            List<NavigatorWindowDocumentItem> docs = new List<NavigatorWindowDocumentItem>();
            listOfDocuments.ForEach((DocumentContent doc) =>
            {
                docs.Add(new NavigatorWindowDocumentItem(doc));
            });

            docs.Sort((NavigatorWindowDocumentItem item1, NavigatorWindowDocumentItem item2) =>
                {
                    if (item1 == item2 ||
                        item1.LastActivation == item2.LastActivation)
                        return 0;
                    return (item1.LastActivation < item2.LastActivation) ? 1 : -1;
                });

            Documents = docs;

            List<DockableContent> listOfContents = _manager.FindContents<DockableContent>();
            List<NavigatorWindowItem> cnts = new List<NavigatorWindowItem>();
            listOfContents.ForEach((DockableContent cnt) =>
            {
                cnts.Add(new NavigatorWindowItem(cnt));
            });

            DockableContents = cnts;


            SelectedContent = Documents.Find((NavigatorWindowDocumentItem docItem) =>
                {
                    return docItem.ItemContent == _manager.ActiveDocument;
                });

            SelectedToolWindow = null;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            if (_manager != null)
            {
                Window mainWindow = Window.GetWindow(_manager);
                if (mainWindow != null)
                {
                    mainWindow.Activate();
                    if (SelectedContent != null)
                    {
                        _manager.Show(SelectedContent.ItemContent as DocumentContent);
                        SelectedContent.ItemContent.SetAsActive();
                    }
                    else if (SelectedToolWindow != null)
                    {
                        _manager.Show(SelectedToolWindow.ItemContent as DockableContent);
                        SelectedToolWindow.ItemContent.SetAsActive();
                    }
                }
            }

            if (!_isClosing)
                CloseThisWindow();//Hide();

            base.OnDeactivated(e);
        }

        void CloseThisWindow()
        {
            Window wndParent = this.Owner;
            Close();
            wndParent.Activate();
        }

        List<NavigatorWindowDocumentItem> _documents = new List<NavigatorWindowDocumentItem>();

        public List<NavigatorWindowDocumentItem> Documents
        {
            get { return _documents; }
            private 
            set 
            { 
                _documents = value; 
                NotifyPropertyChanged("Documents");
            }
        }

        List<NavigatorWindowItem> _tools = new List<NavigatorWindowItem>();

        public List<NavigatorWindowItem> DockableContents
        {
            get { return _tools; }
            private set 
            { 
                _tools = value; 
                NotifyPropertyChanged("DockableContents"); 
            }
        }

        NavigatorWindowDocumentItem _selectedContent;

        public NavigatorWindowDocumentItem SelectedContent
        {
            get 
            {
                return _selectedContent; 
            }
            set
            {
                if (_selectedContent != value)
                {
                     _selectedContent = value;
                    NotifyPropertyChanged("SelectedContent");
                }
            }
        }

        NavigatorWindowItem _toolContent;

        public NavigatorWindowItem SelectedToolWindow
        {
            get
            {
                return _toolContent;
            }
            set
            {
                if (_toolContent != value)
                {
                    _toolContent = value;

                    NotifyPropertyChanged("SelectedToolWindow");

                    SelectedContent = null;
                    Close();// Hide();
                }
            }
        }

        public void MoveNextSelectedContent()
        {
            if (_selectedContent == null)
                return;

            if (Documents.Contains(SelectedContent))
            {
                int indexOfSelecteContent = Documents.IndexOf(_selectedContent);

                if (indexOfSelecteContent == Documents.Count - 1)
                {
                    indexOfSelecteContent = 0;
                }
                else
                    indexOfSelecteContent++;

                SelectedContent = Documents[indexOfSelecteContent];
            }
        }

        bool _isClosing = false;
        protected override void OnClosing(CancelEventArgs e)
        {
            _isClosing = true;

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            //reset documents list to avoid WPF Bug:
            //http://social.msdn.microsoft.com/forums/en/wpf/thread/f3fc5b7e-e035-4821-908c-b6c07e5c7042/
            //http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=321955
            Documents = new List<NavigatorWindowDocumentItem>();

            base.OnClosed(e);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
