using System;
using System.Collections.Generic;
using System.IO;
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

namespace arduino.net
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public partial class SelectionWindow : Window
    {
        public SelectionWindow()
        {
            InitializeComponent();
        }

        public static ConfigSection Show(string title, ICollection<ConfigSection> items, ConfigSection currentValue, string imgDirectory)
        {
            var w = new SelectionWindow();
            InjectImagePaths(items, imgDirectory);
            w.MainListView.ItemsSource = items;
            w.MainListView.SelectedItem = currentValue;
            w.TitleLabel.Content = title;

            var result = w.ShowDialog();

            if (result.HasValue && result.Value == true)
            {
                return w.MainListView.SelectedItem as ConfigSection;
            }

            return null;
        }

        private static void InjectImagePaths(ICollection<ConfigSection> items, string imgDirectory)
        {
            var fullPath = Path.GetFullPath(imgDirectory);

            foreach (var i in items)
            { 
                if (i["image"] != null) continue;

                var imgFile = Path.Combine(fullPath, i.Name + ".png");
                if (!File.Exists(imgFile)) imgFile = Path.Combine(fullPath, "unknown.png");

                i["image"] = imgFile;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ItemDoubleClick(object sender, RoutedEventArgs e)
        {
            OkButton_Click(null, null);
        }
    }
}
