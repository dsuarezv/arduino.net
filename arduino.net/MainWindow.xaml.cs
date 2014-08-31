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
using System.Windows.Shell;
using AurelienRibon.Ui.SyntaxHighlightBox;

namespace arduino.net
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Remove the window border.
            WindowChrome.SetWindowChrome(this, new WindowChrome());

            // Register syntax highlight XMLs in this assembly
            HighlighterManager.Instance.RegisterDefinitions(this.GetType().Assembly);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Configuration.Initialize(@"C:\Program Files (x86)\Arduino");

                Project p = new Project(@"C:\Users\dave\Documents\develop\Arduino\Debugger\Debugger.ino");

                var list = p.GetFileList();

                SampleCodeBox.OpenFile(@"C:\Users\dave\Documents\develop\Arduino\Debugger\soft_debugger.s");
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }


        private void DisplayException(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error");
        }
    }
}
