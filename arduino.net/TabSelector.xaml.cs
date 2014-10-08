using System;
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
    /// Interaction logic for TabSelector.xaml
    /// </summary>
    public partial class TabSelector : UserControl
    {
        private TabControl mTarget;

        public TabControl TargetTabControl
        {
            get { return mTarget; }
            set 
            { 
                mTarget = value;
                TabItemsListBox.ItemsSource = mTarget.Items;
            }
        }

        public TabSelector()
        {
            InitializeComponent();
        }
    }
}
