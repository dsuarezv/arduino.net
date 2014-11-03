using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for WatchesPad.xaml
    /// </summary>
    public partial class WatchesPad : UserControl
    {
        private List<string> mWatchNames = new List<string>();


        public WatchesPad()
        {
            InitializeComponent();
        }


        public void UpdateWatches()
        {
            var values = IdeManager.WatchManager.GetValues(GetWatchNames());
            if (values == null) return;

            MainTreeView.ItemsSource = values;
        }

        private IList<string> GetWatchNames()
        {
            return mWatchNames;
        }


        // __ Control event handlers __________________________________________


        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NewWatchTextBox.Text;
            if (String.IsNullOrWhiteSpace(name)) return;

            name = name.Trim();

            NewWatchTextBox.Text = "";

            mWatchNames.Add(name);

            UpdateWatches();
        }

        private void NewWatchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddNewButton_Click(null, null);
            }
        }
    }
}
