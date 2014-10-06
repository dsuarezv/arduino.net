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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace AvalonDockTest
{
    public class EmployeeInfo
    {
        string name;

        public string FirstName
        {
            get { return name; }
            set { name = value; }
        }
        string lastname;

        public string LastName
        {
            get { return lastname; }
            set { lastname = value; }
        }

        int no;
        public int EmployeeNumber
        {
            get { return no; }
            set { no = value; }
        }

        public EmployeeInfo(int n, string fn, string ln)
        {
            EmployeeNumber = n;
            FirstName = fn;
            LastName = ln;
        }
    }

    public class EmployeeInfoCollection : ObservableCollection<EmployeeInfo>
    {
        public EmployeeInfoCollection()
        {
            Add(new EmployeeInfo(1, "Name1", "LastName1"));
            Add(new EmployeeInfo(2, "Name2", "LastName2"));
            Add(new EmployeeInfo(3, "Name3", "LastName3"));
            Add(new EmployeeInfo(4, "Name4", "LastName4"));
        }
    }
}
