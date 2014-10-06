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
using AvalonDock;
using System.Windows.Controls;
using System.Xml;

namespace AvalonDockTest
{
    public class ErrorListContent : DockableContent
    {
        ListView _errorList;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);


            _errorList = Content as ListView;
        }

        public override void SaveLayout(XmlWriter storeWriter)
        {
            base.SaveLayout(storeWriter);

            if (_errorList == null)
                return;

            GridView gridView = _errorList.View as GridView;

            if (gridView == null)
                return;

            foreach (GridViewColumn col in gridView.Columns)
            {
                storeWriter.WriteStartElement("Column");
                storeWriter.WriteAttributeString("Header", col.Header.ToString());
                storeWriter.WriteAttributeString("Width", XmlConvert.ToString(col.ActualWidth));
                storeWriter.WriteEndElement();
            }
        }

        public override void RestoreLayout(XmlElement contentElement)
        {
            base.RestoreLayout(contentElement);

            if (_errorList == null)
                return;

            GridView gridView = _errorList.View as GridView;

            if (gridView == null)
                return;


            GridViewColumn[] cols = new GridViewColumn[gridView.Columns.Count];
            gridView.Columns.CopyTo(cols, 0);
            gridView.Columns.Clear();

            foreach (XmlElement columnElement in contentElement.ChildNodes)
            {
                foreach (GridViewColumn col in cols)
                {
                    if (col.Header.ToString() == columnElement.GetAttribute("Header"))
                    {
                        col.Width = XmlConvert.ToDouble(columnElement.GetAttribute("Width"));
                        gridView.Columns.Add(col);
                        break;
                    }
                }
            }
        }


    }
}
