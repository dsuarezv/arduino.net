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
        private bool mIsSignalling = false;

        public TabControl TargetTabControl
        {
            get { return mTarget; }
            set 
            {
                UnregisterEvents(mTarget);
                mTarget = value;
                RegisterEvents(mTarget);

                TabItemsListBox.ItemsSource = mTarget.Items;
                
            }
        }

        public TabSelector()
        {
            InitializeComponent();
        }

        private void TabItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mIsSignalling) return;

            var tabItem = TabItemsListBox.SelectedItem as TabItem;
            if (tabItem == null) return;

            mIsSignalling = true;
            tabItem.IsSelected = true;
            mIsSignalling = false;
        }

        private void RegisterEvents(TabControl target)
        {
            if (target == null) return;

            target.SelectionChanged += target_SelectionChanged;
        }

        private void UnregisterEvents(TabControl target)
        {
            if (target == null) return;

            target.SelectionChanged -= target_SelectionChanged;
        }

        private void target_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mIsSignalling) return;

            var tabControl = sender as TabControl;
            if (tabControl == null) return;
            
            var tabItem = tabControl.SelectedItem as TabItem;
            if (tabItem == null) return;

            mIsSignalling = true;
            TabItemsListBox.SelectedItem = tabItem;
            mIsSignalling = false;
        }

    }
}
