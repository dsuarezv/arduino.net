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
using System.Windows.Shapes;

namespace arduino.net
{
    /// <summary>
    /// Interaction logic for GlobalSettingsDialog.xaml
    /// </summary>
    public partial class GlobalSettingsDialog : Window
    {
        public GlobalSettingsDialog()
        {
            InitializeComponent();
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Configuration.Instance;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var bindings = BindingOperations.GetSourceUpdatingBindings(this);

            foreach (var be in bindings) be.UpdateSource();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
