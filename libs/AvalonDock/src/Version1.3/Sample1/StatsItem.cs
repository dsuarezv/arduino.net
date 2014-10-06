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
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sample1
{
    public class StatsItem : INotifyPropertyChanged
    {
        public StatsItem()
        { 

        }


        #region Name

        private string _name = null;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    string oldValue = _name;
                    _name = value;
                    OnNameChanged(oldValue, value);
                    RaisePropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Name property.
        /// </summary>
        protected virtual void OnNameChanged(string oldValue, string newValue)
        {
        }

        #endregion


        #region LineCount

        private int _linesCount = 0;
        public int LinesCount
        {
            get { return _linesCount; }
            set
            {
                if (_linesCount != value)
                {
                    int oldValue = _linesCount;
                    _linesCount = value;
                    OnLinesCountChanged(oldValue, value);
                    RaisePropertyChanged("LinesCount");
                }
            }
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the LinesCount property.
        /// </summary>
        protected virtual void OnLinesCountChanged(int oldValue, int newValue)
        {
        }

        #endregion


        #region WordsCount

        private int _words = 0;
        public int WordsCount
        {
            get { return _words; }
            set
            {
                if (_words != value)
                {
                    int oldValue = _words;
                    _words = value;
                    OnWordsCountChanged(oldValue, value);
                    RaisePropertyChanged("WordsCount");
                }
            }
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the WordsCount property.
        /// </summary>
        protected virtual void OnWordsCountChanged(int oldValue, int newValue)
        {
        }

        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
