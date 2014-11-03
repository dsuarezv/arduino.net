using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public WatchesPad()
        {
            InitializeComponent();
        }


        public void UpdateWatches()
        { 
            var values = IdeManager.WatchManager.GetValues();
            if (values == null) return;

            MainTreeView.ItemsSource = values;
        }


        // __ Control event handlers __________________________________________


        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NewWatchTextBox.Text;
            if (String.IsNullOrWhiteSpace(name)) return;

            name = name.Trim();

            NewWatchTextBox.Text = "";

            IdeManager.WatchManager.SymbolNames.Add(name);

            UpdateWatches();
        }

        private void NewWatchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddNewButton_Click(null, null);
            }
        }

        private void DeleteWatchButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            SymbolInfo si = button.DataContext as SymbolInfo;
            if (si == null) return;

            var symbols = MainTreeView.ItemsSource as ObservableCollection<SymbolInfo>;
            if (symbols == null) return;

            symbols.Remove(si);
            IdeManager.WatchManager.SymbolNames.Remove(si.SymbolName);
        }
    }
}
